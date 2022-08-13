using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using BinaryEncoding;
using JavaAsm.CustomAttributes;
using JavaAsm.Helpers;
using JavaAsm.Instructions.Types;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.Instructions {
    internal static class InstructionListConverter {
        private static AttributeNode GetAttribute(ICollection<AttributeNode> attributes, string name) {
            AttributeNode attribute = attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                attributes.Remove(attribute);
            return attribute;
        }

        public static void ParseCodeAttribute(MethodNode parseTo, ClassReaderState readerState, CodeAttribute codeAttribute) {
            Label.GlobalLabelIndex = 0;

            parseTo.MaxStack = codeAttribute.MaxStack;
            parseTo.MaxLocals = codeAttribute.MaxLocals;
            parseTo.CodeAttributes = codeAttribute.Attributes;

            if (codeAttribute.Code.Length == 0)
                return;

            BootstrapMethodsAttribute bootstrapMethodsAttribute = readerState.ClassNode.Attributes.FirstOrDefault(x => x.Name == PredefinedAttributeNames.BootstrapMethods)?.ParsedAttribute as BootstrapMethodsAttribute;

            Dictionary<long, Instruction> instructions = new Dictionary<long, Instruction>();
            Dictionary<long, Label> labels = new Dictionary<long, Label>();

            bool wideFlag = false;

            MemoryStream codeStream = new MemoryStream(codeAttribute.Code);
            while (codeStream.Position != codeStream.Length) {
                long currentPosition = codeStream.Position;

                Opcode opcode = (Opcode) codeStream.ReadByteFully();
                switch (opcode) {
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
                    case Opcode.IFNONNULL: {
                        int jumpOffset = Binary.BigEndian.ReadInt16(codeStream);
                        instructions.Add(currentPosition, new JumpInstruction(opcode) {
                            Target = labels.GetOrAdd(currentPosition + jumpOffset, () => new Label()),
                            JumpOffset = jumpOffset
                        });
                    }
                        break;
                    case Opcode.JSR_W:
                    case Opcode.GOTO_W: {
                        int jumpOffset = Binary.BigEndian.ReadInt32(codeStream);
                        instructions.Add(currentPosition, new JumpInstruction(opcode == Opcode.JSR_W ? Opcode.JSR : Opcode.GOTO) {
                            Target = labels.GetOrAdd(currentPosition + jumpOffset, () => new Label()),
                            JumpOffset = jumpOffset
                        });
                    }
                        break;

                    case Opcode.LOOKUPSWITCH: {
                        while (codeStream.Position % 4 != 0)
                            codeStream.ReadByteFully();
                        LookupSwitchInstruction lookupSwitchInstruction = new LookupSwitchInstruction {
                            Default = labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), () => new Label())
                        };
                        int nPairs = Binary.BigEndian.ReadInt32(codeStream);
                        lookupSwitchInstruction.MatchLabels.Capacity = nPairs;
                        for (int i = 0; i < nPairs; i++) {
                            lookupSwitchInstruction.MatchLabels.Add(new KeyValuePair<int, Label>(
                                Binary.BigEndian.ReadInt32(codeStream),
                                labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), () => new Label())));
                        }

                        instructions.Add(currentPosition, lookupSwitchInstruction);
                    }
                        break;

                    case Opcode.TABLESWITCH: {
                        while (codeStream.Position % 4 != 0)
                            codeStream.ReadByteFully();
                        TableSwitchInstruction tableSwitchInstruction = new TableSwitchInstruction {
                            Default = labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), () => new Label()),
                            LowValue = Binary.BigEndian.ReadInt32(codeStream),
                            HighValue = Binary.BigEndian.ReadInt32(codeStream)
                        };
                        for (int i = tableSwitchInstruction.LowValue; i <= tableSwitchInstruction.HighValue; i++) {
                            tableSwitchInstruction.Labels.Add(
                                labels.GetOrAdd(currentPosition + Binary.BigEndian.ReadInt32(codeStream), () => new Label()));
                        }

                        instructions.Add(currentPosition, tableSwitchInstruction);
                    }
                        break;

                    case Opcode.INVOKEDYNAMIC:
                        InvokeDynamicEntry callSiteSpecifier = readerState.ConstantPool.GetEntry<InvokeDynamicEntry>(
                            Binary.BigEndian.ReadUInt16(codeStream));
                        if (Binary.BigEndian.ReadUInt16(codeStream) != 0)
                            throw new ArgumentException("INVOKEDYNAMIC 3rd and 4th bytes != 0");
                        instructions.Add(currentPosition, new InvokeDynamicInstruction {
                            Name = callSiteSpecifier.NameAndType.Name.String,
                            Descriptor = MethodDescriptor.Parse(callSiteSpecifier.NameAndType.Descriptor.String),
                            BootstrapMethod = bootstrapMethodsAttribute.BootstrapMethods[callSiteSpecifier.BootstrapMethodAttributeIndex].BootstrapMethodReference,
                            BootstrapMethodArgs = bootstrapMethodsAttribute.BootstrapMethods[callSiteSpecifier.BootstrapMethodAttributeIndex].Arguments,
                        });
                        break;

                    case Opcode.NEWARRAY:
                        instructions.Add(currentPosition, new NewArrayInstruction {
                            ArrayType = (NewArrayTypeCode) codeStream.ReadByteFully()
                        });
                        break;

                    case Opcode.MULTIANEWARRAY:
                        instructions.Add(currentPosition, new MultiANewArrayInstruction {
                            Type = new ClassName(readerState.ConstantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(codeStream)).Name.String),
                            Dimensions = codeStream.ReadByteFully()
                        });
                        break;

                    case Opcode.CHECKCAST:
                    case Opcode.INSTANCEOF:
                    case Opcode.ANEWARRAY:
                    case Opcode.NEW:
                        instructions.Add(currentPosition, new TypeInstruction(opcode) {
                            Type = new ClassName(readerState.ConstantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(codeStream)).Name.String)
                        });
                        break;

                    case Opcode.IINC:
                        if (wideFlag) {
                            instructions.Add(currentPosition - 1, new IncrementInstruction {
                                VariableIndex = Binary.BigEndian.ReadUInt16(codeStream),
                                Value = Binary.BigEndian.ReadInt16(codeStream)
                            });
                            wideFlag = false;
                        }
                        else {
                            instructions.Add(currentPosition, new IncrementInstruction {
                                VariableIndex = codeStream.ReadByteFully(),
                                Value = (sbyte) codeStream.ReadByteFully()
                            });
                        }

                        break;

                    case Opcode.BIPUSH:
                        instructions.Add(currentPosition, new IntegerPushInstruction(opcode) {
                            Value = codeStream.ReadByteFully()
                        });
                        break;
                    case Opcode.SIPUSH:
                        instructions.Add(currentPosition, new IntegerPushInstruction(opcode) {
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
                        if (wideFlag) {
                            variableIndex = Binary.BigEndian.ReadUInt16(codeStream);
                            currentPosition--;
                            wideFlag = false;
                        }
                        else {
                            variableIndex = codeStream.ReadByteFully();
                        }

                        instructions.Add(currentPosition, new VariableInstruction(opcode) {
                            VariableIndex = variableIndex
                        });
                        break;
                    case Opcode.ALOAD_0:
                    case Opcode.ALOAD_1:
                    case Opcode.ALOAD_2:
                    case Opcode.ALOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ALOAD) {
                            VariableIndex = opcode - Opcode.ALOAD_0
                        });
                        break;
                    case Opcode.ASTORE_0:
                    case Opcode.ASTORE_1:
                    case Opcode.ASTORE_2:
                    case Opcode.ASTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ASTORE) {
                            VariableIndex = opcode - Opcode.ASTORE_0
                        });
                        break;
                    case Opcode.DLOAD_0:
                    case Opcode.DLOAD_1:
                    case Opcode.DLOAD_2:
                    case Opcode.DLOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.DLOAD) {
                            VariableIndex = opcode - Opcode.DLOAD_0
                        });
                        break;
                    case Opcode.DSTORE_0:
                    case Opcode.DSTORE_1:
                    case Opcode.DSTORE_2:
                    case Opcode.DSTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.DSTORE) {
                            VariableIndex = opcode - Opcode.DSTORE_0
                        });
                        break;
                    case Opcode.FLOAD_0:
                    case Opcode.FLOAD_1:
                    case Opcode.FLOAD_2:
                    case Opcode.FLOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.FLOAD) {
                            VariableIndex = opcode - Opcode.FLOAD_0
                        });
                        break;
                    case Opcode.FSTORE_0:
                    case Opcode.FSTORE_1:
                    case Opcode.FSTORE_2:
                    case Opcode.FSTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.FSTORE) {
                            VariableIndex = opcode - Opcode.FSTORE_0
                        });
                        break;
                    case Opcode.ILOAD_0:
                    case Opcode.ILOAD_1:
                    case Opcode.ILOAD_2:
                    case Opcode.ILOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ILOAD) {
                            VariableIndex = opcode - Opcode.ILOAD_0
                        });
                        break;
                    case Opcode.ISTORE_0:
                    case Opcode.ISTORE_1:
                    case Opcode.ISTORE_2:
                    case Opcode.ISTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.ISTORE) {
                            VariableIndex = opcode - Opcode.ISTORE_0
                        });
                        break;
                    case Opcode.LLOAD_0:
                    case Opcode.LLOAD_1:
                    case Opcode.LLOAD_2:
                    case Opcode.LLOAD_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.LLOAD) {
                            VariableIndex = opcode - Opcode.LLOAD_0
                        });
                        break;
                    case Opcode.LSTORE_0:
                    case Opcode.LSTORE_1:
                    case Opcode.LSTORE_2:
                    case Opcode.LSTORE_3:
                        instructions.Add(currentPosition, new VariableInstruction(Opcode.LSTORE) {
                            VariableIndex = opcode - Opcode.LSTORE_0
                        });
                        break;

                    case Opcode.INVOKEINTERFACE:
                    case Opcode.INVOKESPECIAL:
                    case Opcode.INVOKESTATIC:
                    case Opcode.INVOKEVIRTUAL: {
                        MethodReferenceEntry methodReferenceEntry;
                        if (opcode == Opcode.INVOKEINTERFACE) {
                            methodReferenceEntry = readerState.ConstantPool.GetEntry<InterfaceMethodReferenceEntry>(
                                Binary.BigEndian.ReadUInt16(codeStream));
                            byte sizeOfArguments = codeStream.ReadByteFully();
                            int requiredSizeOfArguments =
                                MethodDescriptor.Parse(methodReferenceEntry.NameAndType.Descriptor.String).ArgumentTypes.Sum(x => x.SizeOnStack) + 1;
                            if (sizeOfArguments != requiredSizeOfArguments)
                                throw new ArgumentOutOfRangeException(nameof(sizeOfArguments), $"Required size does not equal to provided: {requiredSizeOfArguments} > {sizeOfArguments}");
                            if (codeStream.ReadByteFully() != 0)
                                throw new ArgumentException("INVOKEINTERFACE 4th byte is not 0");
                        }
                        else {
                            methodReferenceEntry = readerState.ConstantPool.GetEntry<MethodReferenceEntry>(
                                Binary.BigEndian.ReadUInt16(codeStream));
                        }

                        instructions.Add(currentPosition, new MethodInstruction(opcode) {
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
                        FieldReferenceEntry fieldReferenceEntry = readerState.ConstantPool.GetEntry<FieldReferenceEntry>(
                            Binary.BigEndian.ReadUInt16(codeStream));
                        instructions.Add(currentPosition, new FieldInstruction(opcode) {
                            Owner = new ClassName(fieldReferenceEntry.Class.Name.String),
                            Descriptor = TypeDescriptor.Parse(fieldReferenceEntry.NameAndType.Descriptor.String),
                            Name = fieldReferenceEntry.NameAndType.Name.String
                        });
                        break;

                    case Opcode.LDC: {
                        Entry constantPoolEntry = readerState.ConstantPool.GetEntry<Entry>(codeStream.ReadByteFully());

                        object ldcVal;
                        switch (constantPoolEntry) {
                            case IntegerEntry ie:
                                ldcVal = ie.Value;
                                break;
                            case FloatEntry fe:
                                ldcVal = fe.Value;
                                break;
                            case StringEntry se:
                                ldcVal = se.Value.String;
                                break;
                            case ClassEntry ce:
                                ldcVal = new ClassName(ce.Name.String);
                                break;
                            case MethodTypeEntry mte:
                                ldcVal = MethodDescriptor.Parse(mte.Descriptor.String);
                                break;
                            case MethodHandleEntry mhe:
                                ldcVal = Handle.FromConstantPool(mhe);
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(constantPoolEntry), $"Tried to {opcode} wrong type of CP entry: {constantPoolEntry.Tag}");
                        }

                        instructions.Add(currentPosition, new LdcInstruction(opcode) {Value = ldcVal});
                    }
                        break;
                    case Opcode.LDC_W:
                    case Opcode.LDC2_W: {
                        Entry constantPoolEntry = readerState.ConstantPool.GetEntry<Entry>(Binary.BigEndian.ReadUInt16(codeStream));
                        object ldcValue;
                        switch (constantPoolEntry) {
                            case IntegerEntry ie when opcode == Opcode.LDC_W:
                                ldcValue = ie.Value;
                                break;
                            case FloatEntry fe when opcode == Opcode.LDC_W:
                                ldcValue = fe.Value;
                                break;
                            case StringEntry se when opcode == Opcode.LDC_W:
                                ldcValue = se.Value.String;
                                break;
                            case ClassEntry ce when opcode == Opcode.LDC_W:
                                ldcValue = new ClassName(ce.Name.String);
                                break;
                            case MethodTypeEntry mte when opcode == Opcode.LDC_W:
                                ldcValue = MethodDescriptor.Parse(mte.Descriptor.String);
                                break;
                            case MethodHandleEntry mhe when opcode == Opcode.LDC_W:
                                ldcValue = new Handle {Descriptor = mhe.ReferenceKind.IsFieldReference() ? TypeDescriptor.Parse(mhe.Reference.NameAndType.Descriptor.String) : (IDescriptor) MethodDescriptor.Parse(mhe.Reference.NameAndType.Descriptor.String), Name = mhe.Reference.NameAndType.Name.String, Owner = new ClassName(mhe.Reference.Class.Name.String)};
                                break;
                            case DoubleEntry de when opcode == Opcode.LDC2_W:
                                ldcValue = de.Value;
                                break;
                            case LongEntry le when opcode == Opcode.LDC2_W:
                                ldcValue = le.Value;
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(constantPoolEntry), $"Tried to {opcode} wrong type of CP entry: {constantPoolEntry.GetType()}");
                        }

                        instructions.Add(currentPosition, new LdcInstruction {
                            Value = ldcValue,
                            Opcode = opcode
                        });
                    }
                        break;

                    case Opcode.None: throw new ArgumentOutOfRangeException(nameof(opcode), "wut?!");

                    case Opcode.BREAKPOINT: throw new ArgumentOutOfRangeException(nameof(opcode), $"Opcode {opcode} is currently not supported");

                    case Opcode.WIDE:
                        wideFlag = true;
                        continue;

                    default: throw new ArgumentOutOfRangeException(nameof(opcode));
                }

                if (wideFlag)
                    throw new Exception("Wide flag hasn't been reset after it has been presented");
            }

            if (labels.Keys.Any(position => !instructions.ContainsKey(position)))
                throw new ArgumentException("Label key is not at the beginning of instruction");

            parseTo.Instructions = new InstructionList();
            parseTo.TryCatches.Capacity = codeAttribute.ExceptionTable.Count;

            foreach (CodeAttribute.ExceptionTableEntry exceptionTableEntry in codeAttribute.ExceptionTable) {
                parseTo.TryCatches.Add(new TryCatchNode {
                    ExceptionClassName = exceptionTableEntry.CatchType,
                    Start = labels.GetOrAdd(exceptionTableEntry.StartPc, () => new Label()),
                    End = labels.GetOrAdd(exceptionTableEntry.EndPc, () => new Label()),
                    Handler = labels.GetOrAdd(exceptionTableEntry.HandlerPc, () => new Label())
                });
            }

            List<KeyValuePair<long, Instruction>> instructionList = instructions.OrderBy(x => x.Key).ToList();
            List<KeyValuePair<long, Label>> labelList = labels.OrderBy(x => x.Key).ToList();
            List<LineNumberTableAttribute.LineNumberTableEntry> lineNumberTable = ((GetAttribute(codeAttribute.Attributes, PredefinedAttributeNames.LineNumberTable)?.ParsedAttribute as LineNumberTableAttribute)?.LineNumberTable ?? new List<LineNumberTableAttribute.LineNumberTableEntry>()).OrderBy(x => x.StartPc).ToList();
            if (lineNumberTable.Any(position => !instructions.ContainsKey(position.StartPc)))
                throw new ArgumentException("Line number is not at the beginning of instruction");

            if (parseTo.LocalVariableTable == null) {
                parseTo.LocalVariableTable = new List<LocalVariableTableAttribute.LocalVariableTableEntry>();
            }
            else {
                parseTo.LocalVariableTable.Clear();
            }

            if (parseTo.LocalVariableTypeTable == null) {
                parseTo.LocalVariableTypeTable = new List<LocalVariableTypeTableAttribute.LocalVariableTypeTableEntry>();
            }
            else {
                parseTo.LocalVariableTypeTable.Clear();
            }

            if (GetAttribute(codeAttribute.Attributes, PredefinedAttributeNames.LocalVariableTable)?.ParsedAttribute is LocalVariableTableAttribute lvt && lvt.LocalVariableTable != null) {
                foreach (LocalVariableTableAttribute.LocalVariableTableEntry entry in lvt.LocalVariableTable) {
                    parseTo.LocalVariableTable.Add(new LocalVariableTableAttribute.LocalVariableTableEntry() {
                        StartPc = entry.StartPc,
                        Length = entry.Length,
                        Name = entry.Name,
                        Descriptor = entry.Descriptor,
                        Index = entry.Index
                    });
                }
            }

            if (GetAttribute(codeAttribute.Attributes, PredefinedAttributeNames.LocalVariableTypeTable)?.ParsedAttribute is LocalVariableTypeTableAttribute lvtt && lvtt.LocalVariableTypeTable != null) {
                foreach (LocalVariableTypeTableAttribute.LocalVariableTypeTableEntry entry in lvtt.LocalVariableTypeTable) {
                    parseTo.LocalVariableTypeTable.Add(new LocalVariableTypeTableAttribute.LocalVariableTypeTableEntry() {
                        Index = entry.Index,
                        StartPc = entry.StartPc,
                        Length = entry.Length,
                        Name = entry.Name,
                        Signature = entry.Signature
                    });
                }
            }

            int labelListPosition = 0;
            int lineNumberTablePosition = 0;
            List<(int Position, StackMapFrame Frame)> stackMapFrames = new List<(int Position, StackMapFrame Frame)>();
            int stackMapFramesPosition = 0;

            {
                List<StackMapTableAttribute.StackMapFrame> stackMapTable = (GetAttribute(codeAttribute.Attributes, PredefinedAttributeNames.StackMapTable)?.ParsedAttribute as StackMapTableAttribute)?.Entries;
                if (stackMapTable != null) {
                    stackMapFrames.Capacity = stackMapTable.Count;
                    int position = 0;
                    bool hasProcessedFirst = false;
                    foreach (StackMapTableAttribute.StackMapFrame entry in stackMapTable) {
                        VerificationElement ConvertVerificationElement(StackMapTableAttribute.VerificationElement sourceVerificationElement) {
                            VerificationElement verificationElement;

                            switch (sourceVerificationElement) {
                                case StackMapTableAttribute.SimpleVerificationElement simpleVerificationElement:
                                    verificationElement = new SimpleVerificationElement((VerificationElementType) simpleVerificationElement.Type);
                                    break;
                                case StackMapTableAttribute.ObjectVerificationElement objectVerificationElement:
                                    verificationElement = new ObjectVerificationElement {
                                        ObjectClass = objectVerificationElement.ObjectClass
                                    };
                                    break;
                                case StackMapTableAttribute.UninitializedVerificationElement uninitializedVerificationElement: {
                                    Instruction newInstruction = instructions[uninitializedVerificationElement.NewInstructionOffset];
                                    if (newInstruction.Opcode != Opcode.NEW)
                                        throw new ArgumentException(
                                            $"New instruction required by verification element is not NEW: {newInstruction.Opcode}", nameof(newInstruction));
                                    verificationElement = new UninitializedVerificationElement {
                                        NewInstruction = (TypeInstruction) newInstruction
                                    };
                                    break;
                                }
                                default: throw new ArgumentOutOfRangeException(nameof(verificationElement));
                            }

                            return verificationElement;
                        }

                        position += entry.OffsetDelta + (hasProcessedFirst ? 1 : 0);

                        StackMapFrame stackMapFrame = new StackMapFrame {
                            Type = (FrameType) entry.Type,
                            ChopK = entry.ChopK
                        };

                        stackMapFrame.Locals.AddRange(entry.Locals.Select(ConvertVerificationElement));
                        stackMapFrame.Stack.AddRange(entry.Stack.Select(ConvertVerificationElement));

                        stackMapFrames.Add((position, stackMapFrame));

                        hasProcessedFirst = true;
                    }
                }
            }

            if (stackMapFrames.Any(frame => !instructions.ContainsKey(frame.Position)))
                throw new ArgumentException("Stack map frame is not at the beginning of instruction");

            {
                GetAttribute(codeAttribute.Attributes, PredefinedAttributeNames.LocalVariableTable);
                GetAttribute(codeAttribute.Attributes, PredefinedAttributeNames.LocalVariableTypeTable);
            }

            foreach (KeyValuePair<long, Instruction> pair in instructionList) {
                while (lineNumberTablePosition < lineNumberTable.Count &&
                       pair.Key >= lineNumberTable[lineNumberTablePosition].StartPc)
                    parseTo.Instructions.Add(new LineNumber {
                        Line = lineNumberTable[lineNumberTablePosition++].LineNumber
                    });
                while (labelListPosition < labelList.Count && pair.Key >= labelList[labelListPosition].Key)
                    parseTo.Instructions.Add(labelList[labelListPosition++].Value);
                while (stackMapFramesPosition < stackMapFrames.Count && pair.Key >= stackMapFrames[stackMapFramesPosition].Position)
                    parseTo.Instructions.Add(stackMapFrames[stackMapFramesPosition++].Frame);
                parseTo.Instructions.Add(pair.Value);
            }

            while (labelListPosition < labelList.Count)
                parseTo.Instructions.Add(labelList[labelListPosition++].Value);
            while (stackMapFramesPosition < stackMapFrames.Count)
                parseTo.Instructions.Add(stackMapFrames[stackMapFramesPosition++].Frame);
        }

        public static CodeAttribute SaveCodeAttribute(MethodNode source, ClassWriterState writerState) {
            CodeAttribute codeAttribute = new CodeAttribute {
                Attributes = source.CodeAttributes,
                MaxLocals = source.MaxLocals,
                MaxStack = source.MaxStack
            };

            foreach (Instruction instruction in source.Instructions) {
                switch (instruction) {
                    case FieldInstruction fieldInstruction:
                        writerState.ConstantPool.Find(new FieldReferenceEntry(
                            new ClassEntry(new Utf8Entry(fieldInstruction.Owner.Name)),
                            new NameAndTypeEntry(new Utf8Entry(fieldInstruction.Name),
                                new Utf8Entry(fieldInstruction.Descriptor.ToString()))));
                        break;
                    case MethodInstruction methodInstruction:
                        writerState.ConstantPool.Find(new MethodReferenceEntry(
                            new ClassEntry(new Utf8Entry(methodInstruction.Owner.Name)),
                            new NameAndTypeEntry(new Utf8Entry(methodInstruction.Name),
                                new Utf8Entry(methodInstruction.Descriptor.ToString()))));
                        break;
                    case TypeInstruction typeInstruction:
                        writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(typeInstruction.Type.Name)));
                        break;
                    case LdcInstruction ldcInstruction:
                        Entry entry;
                        switch (ldcInstruction.Value) {
                            case int integerValue:
                                entry = new IntegerEntry(integerValue);
                                break;
                            case float floatValue:
                                entry = new FloatEntry(floatValue);
                                break;
                            case string stringValue:
                                entry = new StringEntry(new Utf8Entry(stringValue));
                                break;
                            case long longValue:
                                entry = new LongEntry(longValue);
                                break;
                            case double doubleValue:
                                entry = new DoubleEntry(doubleValue);
                                break;
                            case ClassName className:
                                entry = new ClassEntry(new Utf8Entry(className.Name));
                                break;
                            case Handle handle:
                                entry = handle.ToConstantPool();
                                break;
                            case MethodDescriptor methodDescriptor:
                                entry = new MethodTypeEntry(new Utf8Entry(methodDescriptor.ToString()));
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(ldcInstruction.Value), $"Can't encode value of type {ldcInstruction.Value.GetType()}");
                        }

                        writerState.ConstantPool.Find(entry);
                        break;
                    case MultiANewArrayInstruction multiANewArrayInstruction:
                        writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(multiANewArrayInstruction.Type.Name)));
                        break;
                    case InvokeDynamicInstruction invokeDynamicInstruction:
                        AttributeNode bootstrapMethodAttributeNode =
                            writerState.ClassNode.Attributes.FirstOrDefault(x =>
                                x.Name == PredefinedAttributeNames.BootstrapMethods);
                        if (bootstrapMethodAttributeNode == null)
                            writerState.ClassNode.Attributes.Add(bootstrapMethodAttributeNode = new AttributeNode {
                                Name = PredefinedAttributeNames.BootstrapMethods,
                                ParsedAttribute = new BootstrapMethodsAttribute()
                            });
                        if (!(bootstrapMethodAttributeNode.ParsedAttribute is BootstrapMethodsAttribute bootstrapMethodAttribute))
                            throw new Exception("BootstrapMethods attribute exists, but in not-parsed state");
                        BootstrapMethod bootstrapMethod = new BootstrapMethod(invokeDynamicInstruction.BootstrapMethod, invokeDynamicInstruction.BootstrapMethodArgs);
                        if (!bootstrapMethodAttribute.BootstrapMethods.Contains(bootstrapMethod))
                            bootstrapMethodAttribute.BootstrapMethods.Add(bootstrapMethod);
                        writerState.ConstantPool.Find(new InvokeDynamicEntry((ushort) bootstrapMethodAttribute.BootstrapMethods.FindIndex(x => x.Equals(bootstrapMethod)),
                            new NameAndTypeEntry(new Utf8Entry(invokeDynamicInstruction.Name),
                                new Utf8Entry(invokeDynamicInstruction.Descriptor.ToString()))));
                        break;
                    case IncrementInstruction _:
                    case IntegerPushInstruction _:
                    case JumpInstruction _:
                    case LineNumber _:
                    case Label _:
                    case LookupSwitchInstruction _:
                    case NewArrayInstruction _:
                    case SimpleInstruction _:
                    case TableSwitchInstruction _:
                    case VariableInstruction _:
                    case StackMapFrame _:
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(instruction));
                }
            }

            List<LineNumberTableAttribute.LineNumberTableEntry> lineNumbers = new List<LineNumberTableAttribute.LineNumberTableEntry>();
            List<StackMapTableAttribute.StackMapFrame> stackMapFrames = new List<StackMapTableAttribute.StackMapFrame>();
            // Dictionary<int, Dictionary<int, LVTEntry<TypeDescriptor>>> lvtMap = new Dictionary<int, Dictionary<int, LVTEntry<TypeDescriptor>>>();
            // LVTEntry<TypeDescriptor> GetLVTEForFrame(int localVariableIndex) {
            //     if (!lvtMap.TryGetValue(localFrameIndex, out Dictionary<int, LVTEntry<TypeDescriptor>> dict)) {
            //         lvtMap[localFrameIndex] = dict = new Dictionary<int, LVTEntry<TypeDescriptor>>();
            //     }
            //     if (!dict.TryGetValue(localVariableIndex, out LVTEntry<TypeDescriptor> lvte)) {
            //         dict[localVariableIndex] = lvte = new LVTEntry<TypeDescriptor>();
            //     }
            //     return lvte;
            // }

            Dictionary<Instruction, ushort> instructions = new Dictionary<Instruction, ushort>();
            int currentPosition = 0;
            int previousStackMapFramePosition = -1;
            foreach (Instruction instruction in source.Instructions) {
                instructions.Add(instruction, (ushort) currentPosition);
                currentPosition += instruction.Opcode == Opcode.None ? 0 : sizeof(byte);
                switch (instruction) {
                    case FieldInstruction _:
                        currentPosition += sizeof(ushort);
                        break;
                    case MethodInstruction methodInstruction:
                        currentPosition += sizeof(ushort) + (methodInstruction.Opcode == Opcode.INVOKEINTERFACE ? sizeof(byte) + sizeof(byte) : 0);
                        break;
                    case TypeInstruction _:
                        currentPosition += sizeof(ushort);
                        break;
                    case IncrementInstruction incrementInstruction:
                        currentPosition += incrementInstruction.VariableIndex > byte.MaxValue ||
                                           incrementInstruction.Value > sbyte.MaxValue || incrementInstruction.Value < sbyte.MinValue
                            ? sizeof(ushort) * 2 + sizeof(byte)
                            : sizeof(byte) * 2;
                        break;
                    case IntegerPushInstruction integerPushInstruction:
                        currentPosition += integerPushInstruction.Opcode == Opcode.SIPUSH
                            ? sizeof(ushort)
                            : sizeof(byte);
                        break;
                    case JumpInstruction _:
                        currentPosition += sizeof(ushort);
                        break;
                    case LdcInstruction ldcInstruction:
                        if (ldcInstruction.Value is long || ldcInstruction.Value is double) {
                            currentPosition += sizeof(ushort);
                        }
                        else if (ldcInstruction.Opcode == Opcode.LDC2_W || ldcInstruction.Opcode == Opcode.LDC_W) {
                            currentPosition += sizeof(ushort);
                        }
                        else {
                            ushort constantPoolEntryIndex;
                            switch (ldcInstruction.Value) {
                                case int integerValue:
                                    constantPoolEntryIndex = writerState.ConstantPool.Find(new IntegerEntry(integerValue));
                                    break;
                                case float floatValue:
                                    constantPoolEntryIndex = writerState.ConstantPool.Find(new FloatEntry(floatValue));
                                    break;
                                case string stringValue:
                                    constantPoolEntryIndex = writerState.ConstantPool.Find(new StringEntry(new Utf8Entry(stringValue)));
                                    break;
                                case ClassName className:
                                    constantPoolEntryIndex = writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(className.Name)));
                                    break;
                                case Handle handle:
                                    constantPoolEntryIndex = writerState.ConstantPool.Find(handle.ToConstantPool());
                                    break;
                                case MethodDescriptor methodDescriptor:
                                    constantPoolEntryIndex = writerState.ConstantPool.Find(new MethodTypeEntry(new Utf8Entry(methodDescriptor.ToString())));
                                    break;
                                default: throw new ArgumentOutOfRangeException(nameof(ldcInstruction.Value), $"Can't encode value of type {ldcInstruction.Value.GetType()}");
                            }

                            // There's this too... it just has to mutate the LDC instruction, otherwise verification fails
                            // But then this will probably offset the LVT more so :/
                            // it's probably easier to figure out a replacement for the LVT
                            if (constantPoolEntryIndex > byte.MaxValue) {
                                currentPosition += sizeof(ushort);
                                ldcInstruction.Opcode = Opcode.LDC_W;
                            }
                            else {
                                currentPosition += sizeof(byte);
                            }
                        }

                        break;
                    case LineNumber lineNumber:
                        lineNumbers.Add(new LineNumberTableAttribute.LineNumberTableEntry {
                            LineNumber = lineNumber.Line,
                            StartPc = (ushort) currentPosition
                        });
                        break;
                    case StackMapFrame _:
                    case Label _:
                        break;
                    case LookupSwitchInstruction lookupSwitchInstruction:
                        while (currentPosition % 4 != 0)
                            currentPosition++;
                        currentPosition += sizeof(int) + sizeof(int) + (sizeof(int) + sizeof(int)) * lookupSwitchInstruction.MatchLabels.Count;
                        break;
                    case MultiANewArrayInstruction _:
                        currentPosition += sizeof(ushort) + sizeof(byte);
                        break;
                    case NewArrayInstruction _:
                        currentPosition += sizeof(byte);
                        break;
                    case SimpleInstruction _: break;
                    case TableSwitchInstruction tableSwitchInstruction:
                        while (currentPosition % 4 != 0)
                            currentPosition++;
                        currentPosition += sizeof(int) * 3 + sizeof(int) * tableSwitchInstruction.Labels.Count;
                        break;
                    case VariableInstruction varInsn:
                        if (varInsn.Opcode != Opcode.RET && varInsn.VariableIndex < 4)
                            break;

                        currentPosition += varInsn.VariableIndex > byte.MaxValue ? sizeof(ushort) + sizeof(byte) : sizeof(byte);

                        break;
                    case InvokeDynamicInstruction _:
                        currentPosition += sizeof(ushort) + sizeof(ushort);
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(instruction));
                }

                if (currentPosition > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(currentPosition));
            }

            if (lineNumbers.Count > 0) {
                if (codeAttribute.Attributes.Any(x => x.Name == PredefinedAttributeNames.LineNumberTable))
                    throw new ArgumentException($"There is already a {PredefinedAttributeNames.LineNumberTable} attribute");
                codeAttribute.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.LineNumberTable,
                    ParsedAttribute = new LineNumberTableAttribute {
                        LineNumberTable = lineNumbers
                    }
                });
            }

            codeAttribute.ExceptionTable.Capacity = source.TryCatches.Count;
            foreach (TryCatchNode tryCatchNode in source.TryCatches) {
                codeAttribute.ExceptionTable.Add(new CodeAttribute.ExceptionTableEntry {
                    CatchType = tryCatchNode.ExceptionClassName,
                    StartPc = instructions[tryCatchNode.Start],
                    EndPc = instructions[tryCatchNode.End],
                    HandlerPc = instructions[tryCatchNode.Handler]
                });
            }

            if (source.LocalVariableTable != null && source.LocalVariableTable.Count > 0) {
                if (codeAttribute.Attributes.Any(x => x.Name == PredefinedAttributeNames.LocalVariableTable))
                    throw new ArgumentException($"There is already a {PredefinedAttributeNames.LocalVariableTable} attribute");

                foreach (LocalVariableTableAttribute.LocalVariableTableEntry entry in source.LocalVariableTable) {
                    entry.StartPc = Math.Max((ushort) 0, entry.StartPc);
                    uint end = entry.EndPC;
                    if (end > currentPosition) {
                        entry.Length = (ushort) Math.Max(0, entry.Length - (currentPosition - (int) entry.EndPC));
                    }
                }

                codeAttribute.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.LocalVariableTable,
                    ParsedAttribute = new LocalVariableTableAttribute() {
                        LocalVariableTable = source.LocalVariableTable.ToList()
                    }
                });
            }

            if (source.LocalVariableTypeTable != null && source.LocalVariableTypeTable.Count > 0) {
                if (codeAttribute.Attributes.Any(x => x.Name == PredefinedAttributeNames.LocalVariableTypeTable))
                    throw new ArgumentException($"There is already a {PredefinedAttributeNames.LocalVariableTypeTable} attribute");

                foreach (LocalVariableTypeTableAttribute.LocalVariableTypeTableEntry entry in source.LocalVariableTypeTable) {
                    entry.StartPc = Math.Max((ushort) 0, entry.StartPc);
                    uint end = entry.EndPC;
                    if (end > currentPosition) {
                        entry.Length = (ushort) Math.Max(0, entry.Length - (currentPosition - (int) entry.EndPC));
                    }
                }

                codeAttribute.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.LocalVariableTypeTable,
                    ParsedAttribute = new LocalVariableTypeTableAttribute() {
                        LocalVariableTypeTable = source.LocalVariableTypeTable.ToList()
                    }
                });
            }

            MemoryStream codeDataStream = new MemoryStream(currentPosition);

            Dictionary<Instruction, ushort> actualInstructions = new Dictionary<Instruction, ushort>();
            Instruction prevInstruction = null;
            foreach (Instruction instruction in source.Instructions) {
                ushort position = (ushort) codeDataStream.Position;
                actualInstructions[instruction] = position;

                if (position != instructions[instruction]) {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Verification error while writing instructions for {source}");
                    sb.AppendLine($"Wrong position: {position} != {instructions[instruction]}");
                    if (prevInstruction != null) {
                        sb.AppendLine($"    Previous instruction: {prevInstruction.GetType()} -> {prevInstruction}");
                    }

                    if (prevInstruction != null) {
                        sb.AppendLine($"    Current instruction: {instruction.GetType()} -> {instruction}");
                    }

                    sb.AppendLine($"    Verification Instruction map:");
                    foreach (KeyValuePair<Instruction, ushort> entry in instructions) {
                        sb.AppendLine($"        {entry.Key.GetType().Name} ({entry.Key}) at position {entry.Value}");
                    }

                    sb.AppendLine($"    Actual Instruction map:");
                    foreach (KeyValuePair<Instruction, ushort> entry in actualInstructions) {
                        sb.AppendLine($"        {entry.Key.GetType().Name} ({entry.Key}) at position {entry.Value}");
                    }

                    throw new Exception(sb.ToString());
                }

                prevInstruction = instruction;

                switch (instruction) {
                    case FieldInstruction fieldInstruction:
                        codeDataStream.WriteByte((byte) fieldInstruction.Opcode);
                        Binary.BigEndian.Write(codeDataStream, writerState.ConstantPool.Find(new FieldReferenceEntry(
                            new ClassEntry(new Utf8Entry(fieldInstruction.Owner.Name)),
                            new NameAndTypeEntry(new Utf8Entry(fieldInstruction.Name),
                                new Utf8Entry(fieldInstruction.Descriptor.ToString())))));
                        break;
                    case MethodInstruction methodInstruction:
                        codeDataStream.WriteByte((byte) methodInstruction.Opcode);
                        if (methodInstruction.Opcode == Opcode.INVOKEINTERFACE) {
                            Binary.BigEndian.Write(codeDataStream, writerState.ConstantPool.Find(new InterfaceMethodReferenceEntry(
                                new ClassEntry(new Utf8Entry(methodInstruction.Owner.Name)),
                                new NameAndTypeEntry(new Utf8Entry(methodInstruction.Name),
                                    new Utf8Entry(methodInstruction.Descriptor.ToString())))));
                            if (methodInstruction.Descriptor.ArgumentTypes.Sum(x => x.SizeOnStack) + 1 > byte.MaxValue)
                                throw new ArgumentOutOfRangeException(nameof(methodInstruction.Descriptor.ArgumentTypes.Count),
                                    $"Too many arguments: {methodInstruction.Descriptor.ArgumentTypes.Sum(x => x.SizeOnStack) + 1} > {byte.MaxValue}");
                            codeDataStream.WriteByte((byte) (methodInstruction.Descriptor.ArgumentTypes.Sum(x => x.SizeOnStack) + 1));
                            codeDataStream.WriteByte(0);
                        }
                        else {
                            Binary.BigEndian.Write(codeDataStream, writerState.ConstantPool.Find(new MethodReferenceEntry(
                                new ClassEntry(new Utf8Entry(methodInstruction.Owner.Name)),
                                new NameAndTypeEntry(new Utf8Entry(methodInstruction.Name),
                                    new Utf8Entry(methodInstruction.Descriptor.ToString())))));
                        }

                        break;
                    case TypeInstruction typeInstruction:
                        codeDataStream.WriteByte((byte) typeInstruction.Opcode);
                        Binary.BigEndian.Write(codeDataStream,
                            writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(typeInstruction.Type.Name))));
                        break;
                    case IncrementInstruction incrementInstruction:
                        if (incrementInstruction.VariableIndex > byte.MaxValue || incrementInstruction.Value > sbyte.MaxValue || incrementInstruction.Value < sbyte.MinValue)
                            codeDataStream.WriteByte((byte) Opcode.WIDE);
                        codeDataStream.WriteByte((byte) incrementInstruction.Opcode);
                        if (incrementInstruction.VariableIndex > byte.MaxValue ||
                            incrementInstruction.Value > sbyte.MaxValue || incrementInstruction.Value < sbyte.MinValue) {
                            Binary.BigEndian.Write(codeDataStream, incrementInstruction.VariableIndex);
                            Binary.BigEndian.Write(codeDataStream, incrementInstruction.Value);
                        }
                        else {
                            codeDataStream.WriteByte((byte) incrementInstruction.VariableIndex);
                            codeDataStream.WriteByte(unchecked((byte) incrementInstruction.Value));
                        }

                        break;
                    case IntegerPushInstruction integerPushInstruction:
                        codeDataStream.WriteByte((byte) integerPushInstruction.Opcode);
                        if (integerPushInstruction.Opcode == Opcode.SIPUSH)
                            Binary.BigEndian.Write(codeDataStream, integerPushInstruction.Value);
                        else
                            codeDataStream.WriteByte((byte) integerPushInstruction.Value);
                        break;
                    case JumpInstruction jumpInstruction:
                        codeDataStream.WriteByte((byte) jumpInstruction.Opcode);
                        Binary.BigEndian.Write(codeDataStream, (short) (instructions[jumpInstruction.Target] - instructions[jumpInstruction]));
                        break;
                    case LdcInstruction ldcInstruction:
                        ushort constantPoolEntryIndex;
                        switch (ldcInstruction.Value) {
                            case int integerValue:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(new IntegerEntry(integerValue));
                                break;
                            case float floatValue:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(new FloatEntry(floatValue));
                                break;
                            case string stringValue:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(new StringEntry(new Utf8Entry(stringValue)));
                                break;
                            case long longValue:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(new LongEntry(longValue));
                                break;
                            case double doubleValue:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(new DoubleEntry(doubleValue));
                                break;
                            case ClassName className:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(className.Name)));
                                break;
                            case Handle handle:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(handle.ToConstantPool());
                                break;
                            case MethodDescriptor methodDescriptor:
                                constantPoolEntryIndex = writerState.ConstantPool.Find(new MethodTypeEntry(new Utf8Entry(methodDescriptor.ToString())));
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(ldcInstruction.Value), $"Can't encode value of type {ldcInstruction.Value.GetType()}");
                        }

                        if (ldcInstruction.Value is long || ldcInstruction.Value is double) {
                            if (ldcInstruction.Opcode == Opcode.LDC) {
                                throw new InvalidOperationException($"LDC value is a long/double, but it's op-code is not LDC2_W ({ldcInstruction.Opcode}). Value: " + ldcInstruction.Value);
                            }

                            codeDataStream.WriteByte((byte) Opcode.LDC2_W);
                            Binary.BigEndian.Write(codeDataStream, constantPoolEntryIndex);
                        }
                        else {
                            // TODO: What to do here?
                            // If the original const pool index was, for example, below or equal to 255, but the new
                            // index is is above 255, LDC_W will be required and therefore may lead to the LVT (local variable table) being offset
                            // Overall problem: The LVT may contain an entry whose length is near or at the end of the code
                            // Therefore, if LDC_W is converted to LDC, then the LVT length is past the size of the bytecode,
                            // which can cause errors in in asm libraries and possibly in classloading (usually array index out of bounds)

                            // Old code:
                            //  codeDataStream.WriteByte((byte) ((constantPoolEntryIndex > byte.MaxValue) ? Opcode.LDC_W : Opcode.LDC));
                            // New code:
                            //  write the instruction's original opcode only if wide is not required
                            //  the original opcode may be wide anyway. To clarify, this is to prefent an original LDC_W being
                            //  converted to LDC, possilby causing the LVT being offset
                            //  The way the LVT is stored could be modified in this library, so it's not actually a table/map,
                            //  but is actually stored in the instructions themselves maybe? not sure... but the way it is,
                            //  seems very fiddily to actually fix this problem


                            bool isWideRequired = (byte) constantPoolEntryIndex > byte.MaxValue;
                            if (isWideRequired || ldcInstruction.Opcode != Opcode.LDC) {
                                // LDC2_W is only used for long/double... afaik
                                codeDataStream.WriteByte((byte) Opcode.LDC_W);
                                Binary.BigEndian.Write(codeDataStream, constantPoolEntryIndex);
                            }
                            // else if (isWideRequired) {
                            //     // throwing an exception seems bad, because the user has no
                            //     // control over the precise constant pool index
                            //     codeDataStream.WriteByte((byte) Opcode.LDC_W);
                            //     Binary.BigEndian.Write(codeDataStream, constantPoolEntryIndex);
                            // }
                            else {
                                // should always be LDC
                                codeDataStream.WriteByte((byte) ldcInstruction.Opcode);
                                codeDataStream.WriteByte((byte) constantPoolEntryIndex);
                            }
                        }

                        break;
                    case LookupSwitchInstruction lookupSwitchInstruction:
                        codeDataStream.WriteByte((byte) lookupSwitchInstruction.Opcode);
                        while (codeDataStream.Position % 4 != 0)
                            codeDataStream.WriteByte(0);
                        Binary.BigEndian.Write(codeDataStream, instructions[lookupSwitchInstruction.Default] - position);
                        Binary.BigEndian.Write(codeDataStream, lookupSwitchInstruction.MatchLabels.Count);
                        foreach (KeyValuePair<int, Label> pair in lookupSwitchInstruction.MatchLabels) {
                            Binary.BigEndian.Write(codeDataStream, pair.Key);
                            Binary.BigEndian.Write(codeDataStream, instructions[pair.Value] - position);
                        }

                        break;
                    case MultiANewArrayInstruction multiANewArrayInstruction:
                        codeDataStream.WriteByte((byte) multiANewArrayInstruction.Opcode);
                        Binary.BigEndian.Write(codeDataStream, writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(multiANewArrayInstruction.Type.Name))));
                        codeDataStream.WriteByte(multiANewArrayInstruction.Dimensions);
                        break;
                    case NewArrayInstruction newArrayInstruction:
                        codeDataStream.WriteByte((byte) newArrayInstruction.Opcode);
                        codeDataStream.WriteByte((byte) newArrayInstruction.ArrayType);
                        break;
                    case SimpleInstruction simpleInstruction:
                        codeDataStream.WriteByte((byte) simpleInstruction.Opcode);
                        break;
                    case TableSwitchInstruction tableSwitchInstruction:
                        codeDataStream.WriteByte((byte) tableSwitchInstruction.Opcode);
                        while (codeDataStream.Position % 4 != 0)
                            codeDataStream.WriteByte(0);
                        Binary.BigEndian.Write(codeDataStream, instructions[tableSwitchInstruction.Default] - position);
                        Binary.BigEndian.Write(codeDataStream, tableSwitchInstruction.LowValue);
                        Binary.BigEndian.Write(codeDataStream, tableSwitchInstruction.HighValue);
                        if (tableSwitchInstruction.HighValue - tableSwitchInstruction.LowValue + 1 !=
                            tableSwitchInstruction.Labels.Count)
                            throw new ArgumentOutOfRangeException(nameof(tableSwitchInstruction));
                        foreach (Label label in tableSwitchInstruction.Labels)
                            Binary.BigEndian.Write(codeDataStream, instructions[label] - position);
                        break;
                    case VariableInstruction variableInstruction:
                        if (variableInstruction.Opcode != Opcode.RET && variableInstruction.VariableIndex < 4) {
                            // ReSharper disable once SwitchStatementMissingSomeCases
                            switch (variableInstruction.Opcode) {
                                case Opcode.ASTORE:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.ASTORE_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.ISTORE:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.ISTORE_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.FSTORE:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.FSTORE_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.DSTORE:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.DSTORE_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.LSTORE:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.LSTORE_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.ALOAD:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.ALOAD_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.ILOAD:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.ILOAD_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.FLOAD:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.FLOAD_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.DLOAD:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.DLOAD_0 + variableInstruction.VariableIndex));
                                    break;
                                case Opcode.LLOAD:
                                    codeDataStream.WriteByte((byte) ((byte) Opcode.LLOAD_0 + variableInstruction.VariableIndex));
                                    break;
                                default: throw new ArgumentOutOfRangeException(nameof(variableInstruction.Opcode));
                            }
                        }
                        else {
                            if (variableInstruction.VariableIndex > byte.MaxValue)
                                codeDataStream.WriteByte((byte) Opcode.WIDE);
                            codeDataStream.WriteByte((byte) variableInstruction.Opcode);
                            if (variableInstruction.VariableIndex > byte.MaxValue)
                                Binary.BigEndian.Write(codeDataStream, variableInstruction.VariableIndex);
                            else
                                codeDataStream.WriteByte((byte) variableInstruction.VariableIndex);
                        }

                        break;
                    case InvokeDynamicInstruction invokeDynamicInstruction:
                        codeDataStream.WriteByte((byte) invokeDynamicInstruction.Opcode);
                        BootstrapMethodsAttribute bootstrapMethodAttribute =
                            writerState.ClassNode.Attributes.FirstOrDefault(x =>
                                x.Name == PredefinedAttributeNames.BootstrapMethods).ParsedAttribute as BootstrapMethodsAttribute;

                        BootstrapMethod bootstrapMethod = new BootstrapMethod(invokeDynamicInstruction.BootstrapMethod, invokeDynamicInstruction.BootstrapMethodArgs);
                        Binary.BigEndian.Write(codeDataStream, writerState.ConstantPool.Find(new InvokeDynamicEntry(
                            (ushort) bootstrapMethodAttribute.BootstrapMethods.FindIndex(x => x.Equals(bootstrapMethod)),
                            new NameAndTypeEntry(new Utf8Entry(invokeDynamicInstruction.Name),
                                new Utf8Entry(invokeDynamicInstruction.Descriptor.ToString())))));
                        Binary.BigEndian.Write(codeDataStream, (ushort) 0);
                        break;
                    case StackMapFrame stackMapFrame:

                        StackMapTableAttribute.VerificationElement ConvertVerificationElement(VerificationElement sourceVerificationElement) {
                            StackMapTableAttribute.VerificationElement verificationElement;
                            switch (sourceVerificationElement) {
                                case ObjectVerificationElement objectVerificationElement:
                                    verificationElement = new StackMapTableAttribute.ObjectVerificationElement {
                                        ObjectClass = objectVerificationElement.ObjectClass
                                    };
                                    break;
                                case SimpleVerificationElement simpleVerificationElement:
                                    verificationElement = new StackMapTableAttribute.SimpleVerificationElement(
                                        (StackMapTableAttribute.VerificationElementType) simpleVerificationElement.Type);
                                    break;
                                case UninitializedVerificationElement uninitializedVerificationElement:
                                    if (uninitializedVerificationElement.NewInstruction.Opcode != Opcode.NEW)
                                        throw new ArgumentOutOfRangeException(nameof(uninitializedVerificationElement.NewInstruction),
                                            $"New instruction is not NEW: {uninitializedVerificationElement.NewInstruction.Opcode}");
                                    verificationElement = new StackMapTableAttribute.UninitializedVerificationElement {
                                        NewInstructionOffset = instructions[uninitializedVerificationElement.NewInstruction]
                                    };
                                    break;
                                default: throw new ArgumentOutOfRangeException(nameof(sourceVerificationElement));
                            }

                            return verificationElement;
                        }

                        StackMapTableAttribute.StackMapFrame stackMapTableEntry;

                        switch (stackMapFrame.Type) {
                            case FrameType.Same:
                                stackMapTableEntry = new StackMapTableAttribute.StackMapFrame {
                                    Type = StackMapTableAttribute.FrameType.Same
                                };
                                break;
                            case FrameType.SameLocals1StackItem:
                                stackMapTableEntry = new StackMapTableAttribute.StackMapFrame {
                                    Type = StackMapTableAttribute.FrameType.SameLocals1StackItem
                                };
                                stackMapTableEntry.Stack.Add(ConvertVerificationElement(stackMapFrame.Stack[0]));
                                break;
                            case FrameType.Chop:
                                stackMapTableEntry = new StackMapTableAttribute.StackMapFrame {
                                    Type = StackMapTableAttribute.FrameType.Chop,
                                    ChopK = stackMapFrame.ChopK
                                };
                                break;
                            case FrameType.Append:
                                stackMapTableEntry = new StackMapTableAttribute.StackMapFrame {
                                    Type = StackMapTableAttribute.FrameType.Append
                                };
                                stackMapTableEntry.Locals.AddRange(stackMapFrame.Locals.Select(ConvertVerificationElement));
                                break;
                            case FrameType.Full:
                                stackMapTableEntry = new StackMapTableAttribute.StackMapFrame {
                                    Type = StackMapTableAttribute.FrameType.Full
                                };
                                stackMapTableEntry.Locals.AddRange(stackMapFrame.Locals.Select(ConvertVerificationElement));
                                stackMapTableEntry.Stack.AddRange(stackMapFrame.Stack.Select(ConvertVerificationElement));
                                break;
                            default: throw new ArgumentOutOfRangeException(nameof(stackMapFrame.Type));
                        }

                        if (position - previousStackMapFramePosition <= 0)
                            throw new ArgumentOutOfRangeException(nameof(position), $"Wrong stack frame position delta: position({position}) - previousStackMapFramePosition({previousStackMapFramePosition}) ({position - previousStackMapFramePosition}) <= 0");

                        stackMapTableEntry.OffsetDelta = (ushort) (position - previousStackMapFramePosition - 1);
                        stackMapFrames.Add(stackMapTableEntry);
                        previousStackMapFramePosition = position;
                        break;
                    case LineNumber _:
                    case Label _:
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(instruction));
                }
            }

            if (stackMapFrames.Count > 0) {
                if (codeAttribute.Attributes.Any(x => x.Name == PredefinedAttributeNames.StackMapTable))
                    throw new ArgumentException($"There is already a {PredefinedAttributeNames.StackMapTable} attribute");
                codeAttribute.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.StackMapTable,
                    ParsedAttribute = new StackMapTableAttribute {
                        Entries = stackMapFrames
                    }
                });
            }

            codeAttribute.Code = codeDataStream.ToArray();

            return codeAttribute;
        }
    }
}