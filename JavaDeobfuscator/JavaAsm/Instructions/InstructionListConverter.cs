using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.CustomAttributes;
using JavaDeobfuscator.JavaAsm.Instructions.Types;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.Instructions
{
    internal class InstructionListConverter
    {
        public static void ParseInstructionList(MethodNode parseTo, ClassReaderState readerState, CodeAttribute codeAttribute)
        {
            parseTo.MaxStack = codeAttribute.MaxStack;
            parseTo.MaxLocals = codeAttribute.MaxLocals;
            parseTo.CodeAttributes = codeAttribute.Attributes;

            // parsing insns

            var instructionList = new InstructionList();

            using var codeStream = new MemoryStream(codeAttribute.Code);
            while (codeStream.Position != codeStream.Length)
            {
                var opcode = (Opcode) codeStream.ReadByte();
                switch (opcode)
                {
                    case Opcode.NOP:
                    case Opcode.ACONST_NULL:
                    case Opcode.ICONST_M1:
                    case Opcode.ICONST_0:
                    case Opcode.ICONST_1:
                    case Opcode.ICONST_2:
                    case Opcode.ICONST_3:
                    case Opcode.ICONST_4:
                    case Opcode.ICONST_5:
                    case Opcode.LCONST_0:
                    case Opcode.LCONST_1:
                    case Opcode.FCONST_0:
                    case Opcode.FCONST_1:
                    case Opcode.FCONST_2:
                    case Opcode.DCONST_0:
                    case Opcode.DCONST_1:
                    case Opcode.IALOAD:
                    case Opcode.LALOAD:
                    case Opcode.FALOAD:
                    case Opcode.DALOAD:
                    case Opcode.AALOAD:
                    case Opcode.BALOAD:
                    case Opcode.CALOAD:
                    case Opcode.SALOAD:
                    case Opcode.IASTORE:
                    case Opcode.LASTORE:
                    case Opcode.FASTORE:
                    case Opcode.DASTORE:
                    case Opcode.AASTORE:
                    case Opcode.BASTORE:
                    case Opcode.CASTORE:
                    case Opcode.SASTORE:
                    case Opcode.POP:
                    case Opcode.POP2:
                    case Opcode.DUP:
                    case Opcode.DUP_X1:
                    case Opcode.DUP_X2:
                    case Opcode.DUP2:
                    case Opcode.DUP2_X1:
                    case Opcode.DUP2_X2:
                    case Opcode.SWAP:
                    case Opcode.IADD:
                    case Opcode.LADD:
                    case Opcode.FADD:
                    case Opcode.DADD:
                    case Opcode.ISUB:
                    case Opcode.LSUB:
                    case Opcode.FSUB:
                    case Opcode.DSUB:
                    case Opcode.IMUL:
                    case Opcode.LMUL:
                    case Opcode.FMUL:
                    case Opcode.DMUL:
                    case Opcode.IDIV:
                    case Opcode.LDIV:
                    case Opcode.FDIV:
                    case Opcode.DDIV:
                    case Opcode.IREM:
                    case Opcode.LREM:
                    case Opcode.FREM:
                    case Opcode.DREM:
                    case Opcode.INEG:
                    case Opcode.LNEG:
                    case Opcode.FNEG:
                    case Opcode.DNEG:
                    case Opcode.ISHL:
                    case Opcode.LSHL:
                    case Opcode.ISHR:
                    case Opcode.LSHR:
                    case Opcode.IUSHR:
                    case Opcode.LUSHR:
                    case Opcode.IAND:
                    case Opcode.LAND:
                    case Opcode.IOR:
                    case Opcode.LOR:
                    case Opcode.IXOR:
                    case Opcode.LXOR:
                    case Opcode.I2L:
                    case Opcode.I2F:
                    case Opcode.I2D:
                    case Opcode.L2I:
                    case Opcode.L2F:
                    case Opcode.L2D:
                    case Opcode.F2I:
                    case Opcode.F2L:
                    case Opcode.F2D:
                    case Opcode.D2I:
                    case Opcode.D2L:
                    case Opcode.D2F:
                    case Opcode.I2B:
                    case Opcode.I2C:
                    case Opcode.I2S:
                    case Opcode.LCMP:
                    case Opcode.FCMPL:
                    case Opcode.FCMPG:
                    case Opcode.DCMPL:
                    case Opcode.DCMPG:
                    case Opcode.IRETURN:
                    case Opcode.LRETURN:
                    case Opcode.FRETURN:
                    case Opcode.DRETURN:
                    case Opcode.ARETURN:
                    case Opcode.RETURN:
                    case Opcode.ARRAYLENGTH:
                    case Opcode.ATHROW:
                    case Opcode.MONITORENTER:
                    case Opcode.MONITOREXIT:
                        instructionList.Add(new SimpleInstruction(opcode));
                        break;

                    case Opcode.IFEQ:
                    case Opcode.IFNE:
                    case Opcode.IFLT:
                    case Opcode.IFGE:
                    case Opcode.IFGT:
                    case Opcode.IFLE:
                    case Opcode.IF_ICMPEQ:
                    case Opcode.IF_ICMPNE:
                    case Opcode.IF_ICMPLT:
                    case Opcode.IF_ICMPGE:
                    case Opcode.IF_ICMPGT:
                    case Opcode.IF_ICMPLE:
                    case Opcode.IF_ACMPEQ:
                    case Opcode.IF_ACMPNE:
                    case Opcode.GOTO:
                    case Opcode.JSR:
                    case Opcode.IFNULL:
                    case Opcode.IFNONNULL:
                        break;
                    case Opcode.JSR_W:
                    case Opcode.GOTO_W:
                        break;

                    case Opcode.LOOKUPSWITCH:
                        break;

                    case Opcode.TABLESWITCH:
                        break;

                    case Opcode.INVOKEDYNAMIC:
                        break;

                    case Opcode.NEWARRAY:
                        break;

                    case Opcode.MULTIANEWARRAY:
                        break;

                    case Opcode.CHECKCAST:
                    case Opcode.INSTANCEOF:
                    case Opcode.ANEWARRAY:
                    case Opcode.NEW:
                        instructionList.Add(new TypeInstruction(opcode)
                        {
                            Descriptor = TypeDescriptor.Parse(readerState.ConstantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(codeStream)).Name.String) // TODO ?!
                        });
                        break;

                    case Opcode.IINC:
                        break;

                    case Opcode.BIPUSH:
                        instructionList.Add(new IntegerPushInstruction(opcode)
                        {
                            Value = (byte) codeStream.ReadByte()
                        });
                        break;
                    case Opcode.SIPUSH:
                        instructionList.Add(new IntegerPushInstruction(opcode)
                        {
                            Value = Binary.BigEndian.ReadUInt16(codeStream)
                        });
                        break;

                    case Opcode.ALOAD:
                    case Opcode.ASTORE:
                    case Opcode.DLOAD:
                    case Opcode.DSTORE:
                    case Opcode.FLOAD:
                    case Opcode.FSTORE:
                    case Opcode.ILOAD:
                    case Opcode.ISTORE:
                    case Opcode.LLOAD:
                    case Opcode.LSTORE:
                    case Opcode.RET:
                        break;
                    case Opcode.ALOAD_0:
                    case Opcode.ALOAD_1:
                    case Opcode.ALOAD_2:
                    case Opcode.ALOAD_3:
                    case Opcode.ASTORE_0:
                    case Opcode.ASTORE_1:
                    case Opcode.ASTORE_2:
                    case Opcode.ASTORE_3:
                    case Opcode.DLOAD_0:
                    case Opcode.DLOAD_1:
                    case Opcode.DLOAD_2:
                    case Opcode.DLOAD_3:
                    case Opcode.DSTORE_0:
                    case Opcode.DSTORE_1:
                    case Opcode.DSTORE_2:
                    case Opcode.DSTORE_3:
                    case Opcode.FLOAD_0:
                    case Opcode.FLOAD_1:
                    case Opcode.FLOAD_2:
                    case Opcode.FLOAD_3:
                    case Opcode.FSTORE_0:
                    case Opcode.FSTORE_1:
                    case Opcode.FSTORE_2:
                    case Opcode.FSTORE_3:
                    case Opcode.ILOAD_0:
                    case Opcode.ILOAD_1:
                    case Opcode.ILOAD_2:
                    case Opcode.ILOAD_3:
                    case Opcode.ISTORE_0:
                    case Opcode.ISTORE_1:
                    case Opcode.ISTORE_2:
                    case Opcode.ISTORE_3:
                    case Opcode.LLOAD_0:
                    case Opcode.LLOAD_1:
                    case Opcode.LLOAD_2:
                    case Opcode.LLOAD_3:
                    case Opcode.LSTORE_0:
                    case Opcode.LSTORE_1:
                    case Opcode.LSTORE_2:
                    case Opcode.LSTORE_3:
                        if (opcode >= Opcode.ALOAD_0 && opcode <= Opcode.ALOAD_3)
                            instructionList.Add(new VariableInstruction(Opcode.ALOAD)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.ALOAD_0)
                            });
                        if (opcode >= Opcode.ASTORE_0 && opcode <= Opcode.ASTORE_3)
                            instructionList.Add(new VariableInstruction(Opcode.ASTORE)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.ASTORE_0)
                            });
                        if (opcode >= Opcode.DLOAD_0 && opcode <= Opcode.DLOAD_3)
                            instructionList.Add(new VariableInstruction(Opcode.DLOAD)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.DLOAD_0)
                            });
                        if (opcode >= Opcode.DSTORE_0 && opcode <= Opcode.DSTORE_3)
                            instructionList.Add(new VariableInstruction(Opcode.DSTORE)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.DSTORE_0)
                            });
                        if (opcode >= Opcode.FLOAD_0 && opcode <= Opcode.FLOAD_3)
                            instructionList.Add(new VariableInstruction(Opcode.FLOAD)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.FLOAD_0)
                            });
                        if (opcode >= Opcode.FSTORE_0 && opcode <= Opcode.FSTORE_3)
                            instructionList.Add(new VariableInstruction(Opcode.FSTORE)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.FSTORE_0)
                            });
                        if (opcode >= Opcode.ILOAD_0 && opcode <= Opcode.ILOAD_3)
                            instructionList.Add(new VariableInstruction(Opcode.ILOAD)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.ILOAD_0)
                            });
                        if (opcode >= Opcode.ISTORE_0 && opcode <= Opcode.ISTORE_3)
                            instructionList.Add(new VariableInstruction(Opcode.ISTORE)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.ISTORE_0)
                            });
                        if (opcode >= Opcode.LLOAD_0 && opcode <= Opcode.LLOAD_3)
                            instructionList.Add(new VariableInstruction(Opcode.LLOAD)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.LLOAD_0)
                            });
                        if (opcode >= Opcode.LSTORE_0 && opcode <= Opcode.LSTORE_3)
                            instructionList.Add(new VariableInstruction(Opcode.LSTORE)
                            {
                                VariableIndex = (ushort)(opcode - Opcode.LSTORE_0)
                            });
                        break;

                    case Opcode.INVOKEINTERFACE:
                    case Opcode.INVOKESPECIAL:
                    case Opcode.INVOKESTATIC:
                    case Opcode.INVOKEVIRTUAL:
                        break;

                    case Opcode.GETFIELD:
                    case Opcode.GETSTATIC:
                    case Opcode.PUTFIELD:
                    case Opcode.PUTSTATIC:
                        break;

                    case Opcode.LDC:
                        break;
                    case Opcode.LDC_W:
                    case Opcode.LDC2_W:
                        break;

                    case Opcode.None:
                        throw new ArgumentOutOfRangeException(nameof(opcode), "wut?!");

                    case Opcode.BREAKPOINT:
                    case Opcode.IMPDEP1:
                    case Opcode.IMPDEP2:
                        throw new ArgumentOutOfRangeException(nameof(opcode), $"Opcode {opcode} is currently not supported");

                    case Opcode.WIDE:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(opcode));
                }
            }
        }

        public static CodeAttribute GenerateCodeAttribute(MethodNode from, ClassWriterState writerState)
        {
            return new CodeAttribute();
        }
    }
}
