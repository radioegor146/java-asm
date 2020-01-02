using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            if (codeAttribute.Code.Length == 0)
                return;

            var instructions = new Dictionary<long, Instruction>();
            var labels = new Dictionary<long, Label>();

            var wideFlag = false;

            using var codeStream = new MemoryStream(codeAttribute.Code);
            while (codeStream.Position != codeStream.Length)
            {
                var currentPosition = codeStream.Position;

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
                        instructions.Add(currentPosition, new SimpleInstruction(opcode));
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
                        {
                            instructions.Add(currentPosition, new JumpInstruction(opcode)
                            {
                                Target = labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt16(codeStream), new Label())
                            });
                        }
                        break;
                    case Opcode.JSR_W:
                    case Opcode.GOTO_W:
                        {
                            instructions.Add(currentPosition, new JumpInstruction(opcode == Opcode.JSR_W ? Opcode.JSR : Opcode.GOTO)
                            {
                                Target = labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), new Label())
                            });
                        }
                        break;

                    case Opcode.LOOKUPSWITCH:
                        {
                            while (codeStream.Position % 4 != 0)
                                if (codeStream.ReadByte() == -1)
                                    throw new EndOfStreamException();
                            var lookupSwitchInstruction = new LookupSwitchInstruction
                            {
                                Default = labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), new Label())
                            };
                            var nPairs = Binary.BigEndian.ReadInt32(codeStream);
                            lookupSwitchInstruction.MatchLabels.Capacity = nPairs;
                            for (var i = 0; i < nPairs; i++)
                            {
                                lookupSwitchInstruction.MatchLabels.Add(new KeyValuePair<int, Label>(
                                    Binary.BigEndian.ReadInt32(codeStream),
                                    labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), new Label())));
                            }

                            instructions.Add(currentPosition, lookupSwitchInstruction);
                        }
                        break;

                    case Opcode.TABLESWITCH:
                        {
                            while (codeStream.Position % 4 != 0)
                                if (codeStream.ReadByte() == -1)
                                    throw new EndOfStreamException();
                            var tableSwitchInstruction = new TableSwitchInstruction
                            {
                                Default = labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), new Label()),
                                LowValue = Binary.BigEndian.ReadInt32(codeStream), 
                                HighValue = Binary.BigEndian.ReadInt32(codeStream)
                            };
                            for (var i = tableSwitchInstruction.LowValue; i <= tableSwitchInstruction.HighValue; i++)
                            {
                                tableSwitchInstruction.Labels.Add(
                                    labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), new Label()));
                            }

                            instructions.Add(currentPosition, tableSwitchInstruction);
                        }
                        break;

                    case Opcode.INVOKEDYNAMIC:
                        var callSiteSpecifier =
                            readerState.ConstantPool.GetEntry<InvokeDynamicEntry>(
                                Binary.BigEndian.ReadUInt16(codeStream));
                        if (Binary.BigEndian.ReadUInt16(codeStream) != 0)
                            throw new ArgumentException("INVOKEDYNAMIC 3rd and 4th bytes != 0");
                        // TODO
                        instructions.Add(currentPosition, new InvokeDynamicInstruction
                        {

                        });
                        break;

                    case Opcode.NEWARRAY:
                        instructions.Add(currentPosition, new NewArrayInstruction
                        {
                            ArrayType = (NewArrayTypeCode) codeStream.ReadByte()
                        });
                        break;

                    case Opcode.MULTIANEWARRAY:
                        instructions.Add(currentPosition, new MultiANewArrayInstruction
                        {
                            Type = new ClassName(readerState.ConstantPool
                                .GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(codeStream)).Name.String),
                            Dimensions = (byte) codeStream.ReadByte()
                        });
                        break;

                    case Opcode.CHECKCAST:
                    case Opcode.INSTANCEOF:
                    case Opcode.ANEWARRAY:
                    case Opcode.NEW:
                        instructions.Add(currentPosition, new TypeInstruction(opcode)
                        {
                            Type = new ClassName(readerState.ConstantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(codeStream)).Name.String)
                        });
                        break;

                    case Opcode.IINC:
                        if (wideFlag)
                        {
                            instructions.Add(currentPosition, new IncrementInstruction
                            {
                                VariableIndex = Binary.BigEndian.ReadUInt16(codeStream),
                                Value = Binary.BigEndian.ReadUInt16(codeStream)
                            });
                            wideFlag = false;
                        } 
                        else
                        {
                            instructions.Add(currentPosition, new IncrementInstruction
                            {
                                VariableIndex = (byte) codeStream.ReadByte(),
                                Value = (byte) codeStream.ReadByte()
                            });
                        }
                        break;

                    case Opcode.BIPUSH:
                        instructions.Add(currentPosition, new IntegerPushInstruction(opcode)
                        {
                            Value = (byte) codeStream.ReadByte()
                        });
                        break;
                    case Opcode.SIPUSH:
                        instructions.Add(currentPosition, new IntegerPushInstruction(opcode)
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
                        ushort variableIndex;
                        if (wideFlag)
                        {
                            variableIndex = Binary.BigEndian.ReadUInt16(codeStream);
                            wideFlag = false;
                        } 
                        else
                        {
                            variableIndex = (byte) codeStream.ReadByte();
                        }

                        instructions.Add(currentPosition, new VariableInstruction(opcode)
                        {
                            VariableIndex = variableIndex
                        });
                        break;
                    case Opcode.ALOAD_0:
                    case Opcode.ALOAD_1:
                    case Opcode.ALOAD_2:
                    case Opcode.ALOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ALOAD)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.ALOAD_0)
                        });
                        break;
                    case Opcode.ASTORE_0:
                    case Opcode.ASTORE_1:
                    case Opcode.ASTORE_2:
                    case Opcode.ASTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ASTORE)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.ASTORE_0)
                        });
                        break;
                    case Opcode.DLOAD_0:
                    case Opcode.DLOAD_1:
                    case Opcode.DLOAD_2:
                    case Opcode.DLOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.DLOAD)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.DLOAD_0)
                        });
                        break;
                    case Opcode.DSTORE_0:
                    case Opcode.DSTORE_1:
                    case Opcode.DSTORE_2:
                    case Opcode.DSTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.DSTORE)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.DSTORE_0)
                        });
                        break;
                    case Opcode.FLOAD_0:
                    case Opcode.FLOAD_1:
                    case Opcode.FLOAD_2:
                    case Opcode.FLOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.FLOAD)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.FLOAD_0)
                        });
                        break;
                    case Opcode.FSTORE_0:
                    case Opcode.FSTORE_1:
                    case Opcode.FSTORE_2:
                    case Opcode.FSTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.FSTORE)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.FSTORE_0)
                        });
                        break;
                    case Opcode.ILOAD_0:
                    case Opcode.ILOAD_1:
                    case Opcode.ILOAD_2:
                    case Opcode.ILOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ILOAD)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.ILOAD_0)
                        });
                        break;
                    case Opcode.ISTORE_0:
                    case Opcode.ISTORE_1:
                    case Opcode.ISTORE_2:
                    case Opcode.ISTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ISTORE)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.ISTORE_0)
                        });
                        break;
                    case Opcode.LLOAD_0:
                    case Opcode.LLOAD_1:
                    case Opcode.LLOAD_2:
                    case Opcode.LLOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.LLOAD)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.LLOAD_0)
                        });
                        break;
                    case Opcode.LSTORE_0:
                    case Opcode.LSTORE_1:
                    case Opcode.LSTORE_2:
                    case Opcode.LSTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.LSTORE)
                        {
                            VariableIndex = (ushort)(opcode - Opcode.LSTORE_0)
                        });
                        break;

                    case Opcode.INVOKEINTERFACE:
                    case Opcode.INVOKESPECIAL:
                    case Opcode.INVOKESTATIC:
                    case Opcode.INVOKEVIRTUAL:
                        {
                            var methodReferenceEntry = readerState.ConstantPool.GetEntry<MethodReferenceEntry>(
                                Binary.BigEndian.ReadUInt16(codeStream));
                            if (opcode == Opcode.INVOKEINTERFACE)
                            {
                                codeStream.ReadByte();
                                if (codeStream.ReadByte() != 0)
                                    throw new Exception("INVOKEINTERFACE 4th byte is not 0");
                            }

                            instructions.Add(currentPosition, new MethodInstruction(opcode)
                            {
                                Owner = new ClassName(methodReferenceEntry.Class.Name.String),
                                Descriptor = MethodDescriptor.Parse(methodReferenceEntry.NameAndType.Descriptor.String),
                                Name = methodReferenceEntry.NameAndType.Name.String
                            });
                        }
                        break;

                    case Opcode.GETFIELD:
                    case Opcode.GETSTATIC:
                    case Opcode.PUTFIELD:
                    case Opcode.PUTSTATIC:
                        var fieldReferenceEntry = readerState.ConstantPool.GetEntry<FieldReferenceEntry>(
                                Binary.BigEndian.ReadUInt16(codeStream));
                        instructions.Add(currentPosition, new FieldInstruction(opcode)
                        {
                            Owner = new ClassName(fieldReferenceEntry.Class.Name.String),
                            Descriptor = TypeDescriptor.Parse(fieldReferenceEntry.NameAndType.Descriptor.String),
                            Name = fieldReferenceEntry.NameAndType.Name.String
                        });
                        break;

                    case Opcode.LDC:
                        {
                            var constantPoolEntry =
                                readerState.ConstantPool.GetEntry<Entry>((byte) codeStream.ReadByte());
                            instructions.Add(currentPosition, new LdcInsnNode
                            {
                                Value = constantPoolEntry switch
                                {
                                    IntegerEntry integerEntry => (object) integerEntry.Value,
                                    FloatEntry floatEntry => floatEntry.Value,
                                    StringEntry stringEntry => stringEntry.Value.String,
                                    ClassEntry classEntry => new ClassName(classEntry.Name
                                        .String),
                                    MethodTypeEntry methodTypeEntry => MethodDescriptor.Parse(
                                        methodTypeEntry.Descriptor.String),
                                    MethodHandleEntry methodHandleEntry => new Handle
                                    {
                                        Descriptor = methodHandleEntry.ReferenceKind.IsFieldReference()
                                            ? (IDescriptor) TypeDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                                                .Descriptor.String)
                                            : (IDescriptor) MethodDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                                                .Descriptor.String),
                                        Name = methodHandleEntry.Reference.NameAndType.Name.String,
                                        Owner = new ClassName(methodHandleEntry.Reference.Class.Name.String)
                                    },
                                    _ => throw new ArgumentException(
                                        $"Tried to {opcode} wrong type of CP entry: {constantPoolEntry.GetType()}")
                                }
                            });
                        }
                        break;
                    case Opcode.LDC_W:
                    case Opcode.LDC2_W:
                        {
                            var constantPoolEntry =
                                readerState.ConstantPool.GetEntry<Entry>(Binary.BigEndian.ReadUInt16(codeStream));
                            instructions.Add(currentPosition, new LdcInsnNode
                            {
                                Value = constantPoolEntry switch
                                {
                                    IntegerEntry integerEntry when opcode == Opcode.LDC_W => (object) integerEntry.Value,
                                    FloatEntry floatEntry when opcode == Opcode.LDC_W => floatEntry.Value,
                                    StringEntry stringEntry when opcode == Opcode.LDC_W => stringEntry.Value.String,
                                    ClassEntry classEntry when opcode == Opcode.LDC_W => new ClassName(classEntry.Name
                                        .String),
                                    MethodTypeEntry methodTypeEntry when opcode == Opcode.LDC_W => MethodDescriptor.Parse(
                                        methodTypeEntry.Descriptor.String),
                                    MethodHandleEntry methodHandleEntry when opcode == Opcode.LDC_W => new Handle
                                    {
                                        Descriptor = methodHandleEntry.ReferenceKind.IsFieldReference()
                                            ? (IDescriptor) TypeDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                                                .Descriptor.String)
                                            : (IDescriptor) MethodDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                                                .Descriptor.String),
                                        Name = methodHandleEntry.Reference.NameAndType.Name.String,
                                        Owner = new ClassName(methodHandleEntry.Reference.Class.Name.String)
                                    },
                                    DoubleEntry doubleEntry when opcode == Opcode.LDC2_W => doubleEntry.Value,
                                    LongEntry longEntry when opcode == Opcode.LDC2_W => longEntry.Value,
                                    _ => throw new ArgumentException(
                                        $"Tried to {opcode} wrong type of CP entry: {constantPoolEntry.GetType()}")
                                }
                            });
                        }
                        break;

                    case Opcode.None:
                        throw new ArgumentOutOfRangeException(nameof(opcode), "wut?!");

                    case Opcode.BREAKPOINT:
                    case Opcode.IMPDEP1:
                    case Opcode.IMPDEP2:
                        throw new ArgumentOutOfRangeException(nameof(opcode), $"Opcode {opcode} is currently not supported");

                    case Opcode.WIDE:
                        wideFlag = true;
                        continue;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(opcode));
                }

                if (wideFlag)
                    throw new Exception("Wide flag hasn't been reset after it has been presented");
            }

            if (labels.Keys.Any(position => !instructions.ContainsKey(position)))
                throw new ArgumentException("Label key is not at the beginning of instruction");

            parseTo.Instructions = new InstructionList();

            var instructionList = instructions.OrderBy(x => x.Key).ToList();
            var labelList = labels.OrderBy(x => x.Key).ToList();

            var labelListPosition = 0;

            foreach (var (position, instruction) in instructionList)
            {
                while (labelListPosition < labelList.Count && position >= labelList[labelListPosition].Key)
                    parseTo.Instructions.Add(labelList[labelListPosition++].Value);
                parseTo.Instructions.Add(instruction);
            }
        }

        public static CodeAttribute GenerateCodeAttribute(MethodNode from, ClassWriterState writerState)
        {
            return new CodeAttribute();
        }
    }

    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
            return dictionary[key];
        }
    }
}
