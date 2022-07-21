using System;
using System.Collections.Generic;
using System.Linq;
using JavaAsm.Helpers;
using JavaAsm.Instructions;
using JavaAsm.Instructions.Types;

namespace JavaAsm.Commons {
    /// <summary>
    /// Helper class for different operations with methods (computing sizes, frames, etc)
    /// </summary>
    public static class MethodHelper {
        /// <summary>
        /// Computers max number of locals and stack
        /// </summary>
        /// <param name="methodNode">Method to compute for</param>
        /// <returns>ValueTuple of max number of locals and stack</returns>
        public static (ushort MaxLocals, ushort MaxStack) ComputeMaxStackAndLocals(MethodNode methodNode) {
            if (methodNode.Instructions == null || methodNode.Access.HasFlag(MethodAccessModifiers.Native) ||
                methodNode.Access.HasFlag(MethodAccessModifiers.Abstract))
                throw new ArgumentOutOfRangeException(nameof(methodNode),
                    "Can't compute stack and locals for native or abstract method");

            int maxLocalIndex = Math.Max(methodNode.Instructions.Any(x => x is VariableInstruction)
                    ? methodNode.Instructions.Where(x => x is VariableInstruction).Max(x => ((VariableInstruction) x).VariableIndex +
                                                                                            (x.Opcode.In(Opcode.LLOAD, Opcode.LSTORE, Opcode.DLOAD, Opcode.DSTORE) ? 1 : 0)) + 1
                    : 0,
                methodNode.Descriptor.ArgumentTypes.Sum(x => x.SizeOnStack) + (methodNode.Access.HasFlag(MethodAccessModifiers.Static) ? 0 : 1));
            if (maxLocalIndex > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(maxLocalIndex),
                    $"Max local index is larger that maximum possible amount: {maxLocalIndex} > {ushort.MaxValue}");

            ushort maxLocals = (ushort) maxLocalIndex;

            Queue<Instruction> queueToCompute = new Queue<Instruction>();
            queueToCompute.Enqueue(methodNode.Instructions.First);

            Dictionary<Instruction, ushort> stackSizes = new Dictionary<Instruction, ushort> {
                {methodNode.Instructions.First, 0}
            };
            foreach (TryCatchNode tryCatchBlock in methodNode.TryCatches) {
                if (tryCatchBlock.Handler.OwnerList != methodNode.Instructions)
                    throw new Exception("TryCatch block handler label does not belongs to methodNode's instructions list");
                if (stackSizes.TryAdd<Instruction, ushort>(tryCatchBlock.Handler, 1))
                    queueToCompute.Enqueue(tryCatchBlock.Handler);
            }

            void CheckStackSizeAndThrow(int stackSize) {
                if (stackSize < 0)
                    throw new ArgumentOutOfRangeException(nameof(stackSize),
                        $"Stack underflow: {stackSize} < {ushort.MinValue}");
                if (stackSize > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(stackSize),
                        $"Stack overflow: {stackSize} > {ushort.MaxValue}");
            }

            void EnqueueInstruction(Instruction instruction, int newStackSize) {
                CheckStackSizeAndThrow(newStackSize);
                if (stackSizes.TryAdd(instruction, (ushort) newStackSize))
                    queueToCompute.Enqueue(instruction);
                else if (stackSizes[instruction] != newStackSize)
                    throw new Exception($"Stack size difference on instruction {instruction}");
            }

            while (queueToCompute.Any()) {
                Instruction currentInstruction = queueToCompute.Dequeue();
                int newStackSize = stackSizes[currentInstruction];
                switch (currentInstruction) {
                    case FieldInstruction fieldInstruction:
                        if (fieldInstruction.Opcode.In(Opcode.GETFIELD, Opcode.PUTFIELD)) {
                            newStackSize--;
                            CheckStackSizeAndThrow(newStackSize);
                        }

                        if (fieldInstruction.Opcode.In(Opcode.GETFIELD, Opcode.GETSTATIC))
                            newStackSize += fieldInstruction.Descriptor.SizeOnStack;
                        else
                            newStackSize -= fieldInstruction.Descriptor.SizeOnStack;
                        break;
                    case IntegerPushInstruction _:
                        newStackSize++;
                        break;
                    case InvokeDynamicInstruction invokeDynamicInstruction:
                        newStackSize -= invokeDynamicInstruction.Descriptor.ArgumentTypes.Sum(x => x.SizeOnStack);
                        CheckStackSizeAndThrow(newStackSize);
                        newStackSize += invokeDynamicInstruction.Descriptor.ReturnType.SizeOnStack;
                        break;
                    case JumpInstruction jumpInstruction:
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (jumpInstruction.Opcode) {
                            case Opcode.IFEQ:
                            case Opcode.IFNE:
                            case Opcode.IFLT:
                            case Opcode.IFGE:
                            case Opcode.IFGT:
                            case Opcode.IFLE:
                            case Opcode.IFNULL:
                            case Opcode.IFNONNULL:
                                newStackSize--;
                                EnqueueInstruction(jumpInstruction.Target, newStackSize);
                                break;
                            case Opcode.IF_ICMPEQ:
                            case Opcode.IF_ICMPNE:
                            case Opcode.IF_ICMPLT:
                            case Opcode.IF_ICMPGE:
                            case Opcode.IF_ICMPGT:
                            case Opcode.IF_ICMPLE:
                            case Opcode.IF_ACMPEQ:
                            case Opcode.IF_ACMPNE:
                                newStackSize -= 2;
                                EnqueueInstruction(jumpInstruction.Target, newStackSize);
                                break;
                            case Opcode.JSR:
                                newStackSize++;
                                EnqueueInstruction(jumpInstruction.Target, newStackSize);
                                continue;
                            case Opcode.GOTO:
                                EnqueueInstruction(jumpInstruction.Target, newStackSize);
                                continue;
                            default: throw new ArgumentOutOfRangeException(nameof(jumpInstruction.Opcode));
                        }

                        break;
                    case LdcInstruction ldcInstruction:
                        newStackSize += ldcInstruction.Value is long || ldcInstruction.Value is double ? 2 : 1;
                        break;
                    case LookupSwitchInstruction lookupSwitchInstruction:
                        newStackSize--;
                        EnqueueInstruction(lookupSwitchInstruction.Default, newStackSize);
                        foreach (KeyValuePair<int, Label> jumpPoint in lookupSwitchInstruction.MatchLabels)
                            EnqueueInstruction(jumpPoint.Value, newStackSize);
                        continue;
                    case MethodInstruction methodInstruction:
                        if (methodInstruction.Opcode != Opcode.INVOKESTATIC) {
                            newStackSize--;
                            CheckStackSizeAndThrow(newStackSize);
                        }

                        newStackSize -= methodInstruction.Descriptor.ArgumentTypes.Sum(x => x.SizeOnStack);
                        CheckStackSizeAndThrow(newStackSize);
                        newStackSize += methodInstruction.Descriptor.ReturnType.SizeOnStack;
                        break;
                    case MultiANewArrayInstruction multiANewArrayInstruction:
                        newStackSize -= multiANewArrayInstruction.Dimensions;
                        CheckStackSizeAndThrow(newStackSize);
                        newStackSize++;
                        break;
                    case NewArrayInstruction _:
                        newStackSize--;
                        CheckStackSizeAndThrow(newStackSize);
                        newStackSize++;
                        break;
                    case SimpleInstruction simpleInstruction:
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (simpleInstruction.Opcode) {
                            case Opcode.NOP: break;
                            case Opcode.ACONST_NULL:
                            case Opcode.ICONST_M1:
                            case Opcode.ICONST_0:
                            case Opcode.ICONST_1:
                            case Opcode.ICONST_2:
                            case Opcode.ICONST_3:
                            case Opcode.ICONST_4:
                            case Opcode.ICONST_5:
                            case Opcode.FCONST_0:
                            case Opcode.FCONST_1:
                            case Opcode.FCONST_2:
                                newStackSize++;
                                break;
                            case Opcode.LCONST_0:
                            case Opcode.LCONST_1:
                            case Opcode.DCONST_0:
                            case Opcode.DCONST_1:
                                newStackSize += 2;
                                break;
                            case Opcode.IALOAD:
                            case Opcode.FALOAD:
                            case Opcode.AALOAD:
                            case Opcode.BALOAD:
                            case Opcode.CALOAD:
                            case Opcode.SALOAD:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize++;
                                break;
                            case Opcode.LALOAD:
                            case Opcode.DALOAD:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 2;
                                break;
                            case Opcode.DUP:
                            case Opcode.I2L:
                            case Opcode.I2D:
                            case Opcode.F2L:
                            case Opcode.F2D:
                                newStackSize--;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 2;
                                break;
                            case Opcode.IASTORE:
                            case Opcode.FASTORE:
                            case Opcode.AASTORE:
                            case Opcode.BASTORE:
                            case Opcode.CASTORE:
                            case Opcode.SASTORE:
                                newStackSize -= 3;
                                break;
                            case Opcode.POP2:
                                newStackSize -= 2;
                                break;
                            case Opcode.DASTORE:
                            case Opcode.LASTORE:
                                newStackSize -= 4;
                                break;
                            case Opcode.POP:
                            case Opcode.MONITORENTER:
                            case Opcode.MONITOREXIT:
                                newStackSize--;
                                break;
                            case Opcode.DUP_X1:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 3;
                                break;
                            case Opcode.DUP_X2:
                                newStackSize -= 3;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 4;
                                break;
                            case Opcode.DUP2:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 4;
                                break;
                            case Opcode.DUP2_X1:
                                newStackSize -= 3;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 5;
                                break;
                            case Opcode.DUP2_X2:
                                newStackSize -= 4;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 6;
                                break;
                            case Opcode.SWAP:
                            case Opcode.LNEG:
                            case Opcode.DNEG:
                            case Opcode.L2D:
                            case Opcode.D2L:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 2;
                                break;
                            case Opcode.IADD:
                            case Opcode.FADD:
                            case Opcode.ISUB:
                            case Opcode.FSUB:
                            case Opcode.IMUL:
                            case Opcode.FMUL:
                            case Opcode.IDIV:
                            case Opcode.FDIV:
                            case Opcode.IREM:
                            case Opcode.FREM:
                            case Opcode.IAND:
                            case Opcode.IOR:
                            case Opcode.IXOR:
                            case Opcode.ISHL:
                            case Opcode.ISHR:
                            case Opcode.IUSHR:
                            case Opcode.FCMPL:
                            case Opcode.FCMPG:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize++;
                                break;
                            case Opcode.LADD:
                            case Opcode.DADD:
                            case Opcode.LSUB:
                            case Opcode.DSUB:
                            case Opcode.LMUL:
                            case Opcode.DMUL:
                            case Opcode.LDIV:
                            case Opcode.DDIV:
                            case Opcode.LREM:
                            case Opcode.DREM:
                            case Opcode.LAND:
                            case Opcode.LOR:
                            case Opcode.LXOR:
                                newStackSize -= 4;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 2;
                                break;
                            case Opcode.INEG:
                            case Opcode.FNEG:
                            case Opcode.I2F:
                            case Opcode.F2I:
                            case Opcode.I2B:
                            case Opcode.I2C:
                            case Opcode.I2S:
                            case Opcode.ARRAYLENGTH:
                                newStackSize--;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize++;
                                break;
                            case Opcode.LSHL:
                            case Opcode.LSHR:
                            case Opcode.LUSHR:
                                newStackSize -= 3;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize += 2;
                                break;
                            case Opcode.L2I:
                            case Opcode.L2F:
                            case Opcode.D2I:
                            case Opcode.D2F:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize++;
                                break;
                            case Opcode.LCMP:
                            case Opcode.DCMPL:
                            case Opcode.DCMPG:
                                newStackSize -= 4;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize++;
                                break;
                            case Opcode.ARETURN:
                            case Opcode.IRETURN:
                            case Opcode.FRETURN:
                            case Opcode.ATHROW:
                                newStackSize--;
                                CheckStackSizeAndThrow(newStackSize);
                                continue;
                            case Opcode.LRETURN:
                            case Opcode.DRETURN:
                                newStackSize -= 2;
                                CheckStackSizeAndThrow(newStackSize);
                                continue;
                            case Opcode.RETURN: continue;
                            default: throw new ArgumentOutOfRangeException(nameof(simpleInstruction.Opcode));
                        }

                        break;
                    case TableSwitchInstruction tableSwitchInstruction:
                        newStackSize--;
                        CheckStackSizeAndThrow(newStackSize);
                        EnqueueInstruction(tableSwitchInstruction.Default, newStackSize);
                        foreach (Label jumpPoint in tableSwitchInstruction.Labels)
                            EnqueueInstruction(jumpPoint, newStackSize);
                        continue;
                    case TypeInstruction typeInstruction:
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (typeInstruction.Opcode) {
                            case Opcode.NEW:
                                newStackSize++;
                                break;
                            case Opcode.ANEWARRAY:
                            case Opcode.CHECKCAST:
                            case Opcode.INSTANCEOF:
                                newStackSize--;
                                CheckStackSizeAndThrow(newStackSize);
                                newStackSize++;
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(typeInstruction.Opcode));
                        }

                        break;
                    case VariableInstruction variableInstruction:
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (variableInstruction.Opcode) {
                            case Opcode.ALOAD:
                            case Opcode.ILOAD:
                            case Opcode.FLOAD:
                                newStackSize++;
                                break;
                            case Opcode.LLOAD:
                            case Opcode.DLOAD:
                                newStackSize += 2;
                                break;
                            case Opcode.ASTORE:
                            case Opcode.ISTORE:
                            case Opcode.FSTORE:
                                newStackSize--;
                                break;
                            case Opcode.LSTORE:
                            case Opcode.DSTORE:
                                newStackSize -= 2;
                                break;
                            case Opcode.RET: break;
                            default: throw new ArgumentOutOfRangeException(nameof(variableInstruction.Opcode));
                        }

                        break;
                    case StackMapFrame _:
                    case IncrementInstruction _:
                    case LineNumber _:
                    case Label _:
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(currentInstruction));
                }

                EnqueueInstruction(currentInstruction.Next, newStackSize);
            }

            return (maxLocals, stackSizes.Max(x => x.Value));
        }

        /// <summary>
        /// Computes stack frames
        /// </summary>
        /// <param name="methodNode">Method to compute for</param>
        public static void ComputeStackMapFrames(MethodNode methodNode) {
            throw new NotImplementedException();
        }
    }
}