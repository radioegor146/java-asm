using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes {
    public class StackMapTableAttribute : CustomAttribute {
        public enum VerificationElementType {
            Top,
            Integer,
            Float,
            Long,
            Double,
            Null,
            UnitializedThis,
            Object,
            Unitialized
        }

        public abstract class VerificationElement {
            public abstract VerificationElementType Type { get; }
        }

        public class SimpleVerificationElement : VerificationElement {
            public override VerificationElementType Type { get; }

            public SimpleVerificationElement(VerificationElementType type) {
                type.CheckInAndThrow(nameof(type), VerificationElementType.Top, VerificationElementType.Integer,
                    VerificationElementType.Float, VerificationElementType.Long, VerificationElementType.Double, VerificationElementType.Null, VerificationElementType.UnitializedThis);
                this.Type = type;
            }
        }

        public class ObjectVerificationElement : VerificationElement {
            public override VerificationElementType Type => VerificationElementType.Object;

            public ClassName ObjectClass { get; set; }
        }

        public class UninitializedVerificationElement : VerificationElement {
            public override VerificationElementType Type => VerificationElementType.Unitialized;

            public ushort NewInstructionOffset { get; set; }
        }

        public enum FrameType {
            Same,
            SameLocals1StackItem,
            Chop,
            Append,
            Full
        }

        public class StackMapFrame {
            public FrameType Type { get; set; }

            public ushort OffsetDelta { get; set; }

            public List<VerificationElement> Stack { get; } = new List<VerificationElement>();

            public List<VerificationElement> Locals { get; } = new List<VerificationElement>();

            public byte? ChopK { get; set; }
        }

        public List<StackMapFrame> Entries { get; set; } = new List<StackMapFrame>();

        internal static void WriteVerificationElement(Stream stream, ClassWriterState writerState, VerificationElement verificationElement) {
            stream.WriteByte((byte) verificationElement.Type);
            switch (verificationElement) {
                case ObjectVerificationElement objectVerificationElement:
                    Binary.BigEndian.Write(stream,
                        writerState.ConstantPool.Find(
                            new ClassEntry(new Utf8Entry(objectVerificationElement.ObjectClass.Name))));
                    break;
                case UninitializedVerificationElement uninitializedVerificationElement:
                    Binary.BigEndian.Write(stream, uninitializedVerificationElement.NewInstructionOffset);
                    break;
                case SimpleVerificationElement _: break;
                default: throw new ArgumentOutOfRangeException(nameof(verificationElement));
            }
        }

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.Entries.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Entries.Count), $"Too many entries for StackMapTable: {this.Entries.Count} > {ushort.MaxValue}");

            Binary.BigEndian.Write(attributeDataStream, (ushort) this.Entries.Count);

            foreach (StackMapFrame entry in this.Entries) {
                switch (entry.Type) {
                    case FrameType.Same:
                        if (entry.OffsetDelta < 64)
                            attributeDataStream.WriteByte((byte) entry.OffsetDelta);
                        else {
                            attributeDataStream.WriteByte(251);
                            Binary.BigEndian.Write(attributeDataStream, entry.OffsetDelta);
                        }

                        break;
                    case FrameType.SameLocals1StackItem:
                        if (entry.OffsetDelta < 64)
                            attributeDataStream.WriteByte((byte) (entry.OffsetDelta + 64));
                        else {
                            attributeDataStream.WriteByte(247);
                            Binary.BigEndian.Write(attributeDataStream, entry.OffsetDelta);
                        }

                        WriteVerificationElement(attributeDataStream, writerState, entry.Stack[0]);
                        break;
                    case FrameType.Chop:
                        if (entry.ChopK == null)
                            throw new ArgumentNullException(nameof(entry.ChopK));
                        if (entry.ChopK < 1 || entry.ChopK > 3)
                            throw new ArgumentOutOfRangeException(nameof(entry.ChopK),
                                $"Chop K was < 1 || > 3: {entry.ChopK}");

                        attributeDataStream.WriteByte((byte) (251 - entry.ChopK));
                        Binary.BigEndian.Write(attributeDataStream, entry.OffsetDelta);
                        break;
                    case FrameType.Append:
                        if (entry.Locals.Count < 1 || entry.Locals.Count > 3)
                            throw new ArgumentOutOfRangeException(nameof(entry.Locals),
                                $"Number of locals was < 1 || > 3: {entry.Locals}");
                        attributeDataStream.WriteByte((byte) (251 + entry.Locals.Count));
                        Binary.BigEndian.Write(attributeDataStream, entry.OffsetDelta);
                        foreach (VerificationElement verificationElement in entry.Locals)
                            WriteVerificationElement(attributeDataStream, writerState, verificationElement);
                        break;
                    case FrameType.Full:
                        attributeDataStream.WriteByte(255);
                        Binary.BigEndian.Write(attributeDataStream, entry.OffsetDelta);

                        if (entry.Locals.Count > ushort.MaxValue)
                            throw new ArgumentOutOfRangeException(nameof(entry.Locals.Count),
                                $"Too many entries in frame's locals: {entry.Locals.Count} > {ushort.MaxValue}");
                        Binary.BigEndian.Write(attributeDataStream, (ushort) entry.Locals.Count);
                        foreach (VerificationElement verificationElement in entry.Locals)
                            WriteVerificationElement(attributeDataStream, writerState, verificationElement);

                        if (entry.Stack.Count > ushort.MaxValue)
                            throw new ArgumentOutOfRangeException(nameof(entry.Stack.Count),
                                $"Too many entries in frame's stack: {entry.Stack.Count} > {ushort.MaxValue}");
                        Binary.BigEndian.Write(attributeDataStream, (ushort) entry.Stack.Count);
                        foreach (VerificationElement verificationElement in entry.Stack)
                            WriteVerificationElement(attributeDataStream, writerState, verificationElement);
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(entry.Type));
                }
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class StackMapTableAttributeFactory : ICustomAttributeFactory<StackMapTableAttribute> {
        private static StackMapTableAttribute.VerificationElement ReadVerificationElement(Stream stream, ClassReaderState readerState) {
            StackMapTableAttribute.VerificationElementType verificationElementType = (StackMapTableAttribute.VerificationElementType) stream.ReadByteFully();
            switch (verificationElementType) {
                case StackMapTableAttribute.VerificationElementType.Top: return new StackMapTableAttribute.SimpleVerificationElement(verificationElementType);
                case StackMapTableAttribute.VerificationElementType.Integer:
                case StackMapTableAttribute.VerificationElementType.Float:
                case StackMapTableAttribute.VerificationElementType.Long:
                case StackMapTableAttribute.VerificationElementType.Double:
                case StackMapTableAttribute.VerificationElementType.Null:
                case StackMapTableAttribute.VerificationElementType.UnitializedThis:
                    return new StackMapTableAttribute.SimpleVerificationElement(verificationElementType);
                case StackMapTableAttribute.VerificationElementType.Object: return new StackMapTableAttribute.ObjectVerificationElement {ObjectClass = new ClassName(readerState.ConstantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(stream)).Name.String)};
                case StackMapTableAttribute.VerificationElementType.Unitialized: return new StackMapTableAttribute.UninitializedVerificationElement {NewInstructionOffset = Binary.BigEndian.ReadUInt16(stream)};
                default: throw new ArgumentOutOfRangeException(nameof(verificationElementType));
            }
        }

        public StackMapTableAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            StackMapTableAttribute attribute = new StackMapTableAttribute();

            ushort stackMapTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Entries.Capacity = stackMapTableSize;
            for (int i = 0; i < stackMapTableSize; i++) {
                StackMapTableAttribute.StackMapFrame stackMapFrame;
                byte frameTypeByte = attributeDataStream.ReadByteFully();
                if (frameTypeByte < 64) {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame {
                        Type = StackMapTableAttribute.FrameType.Same,
                        OffsetDelta = frameTypeByte
                    };
                }
                else if (frameTypeByte < 128) {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame {
                        Type = StackMapTableAttribute.FrameType.SameLocals1StackItem,
                        OffsetDelta = (ushort) (frameTypeByte - 64)
                    };

                    stackMapFrame.Stack.Add(ReadVerificationElement(attributeDataStream, readerState));
                }
                else if (frameTypeByte == 247) {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame {
                        Type = StackMapTableAttribute.FrameType.SameLocals1StackItem,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };

                    stackMapFrame.Stack.Add(ReadVerificationElement(attributeDataStream, readerState));
                }
                else if (frameTypeByte < 251) {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame {
                        Type = StackMapTableAttribute.FrameType.Chop,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream),
                        ChopK = (byte) (251 - frameTypeByte)
                    };
                }
                else if (frameTypeByte == 251) {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame {
                        Type = StackMapTableAttribute.FrameType.Same,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };
                }
                else if (frameTypeByte < 255) {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame {
                        Type = StackMapTableAttribute.FrameType.Append,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };

                    for (int j = 0; j < frameTypeByte - 251; j++) {
                        stackMapFrame.Locals.Add(ReadVerificationElement(attributeDataStream, readerState));
                    }
                }
                else if (frameTypeByte == 255) {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame {
                        Type = StackMapTableAttribute.FrameType.Full,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };

                    ushort localsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
                    for (int j = 0; j < localsCount; j++)
                        stackMapFrame.Locals.Add(ReadVerificationElement(attributeDataStream, readerState));
                    ushort stackCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
                    for (int j = 0; j < stackCount; j++)
                        stackMapFrame.Stack.Add(ReadVerificationElement(attributeDataStream, readerState));
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(frameTypeByte));

                attribute.Entries.Add(stackMapFrame);
            }

            return attribute;
        }
    }
}