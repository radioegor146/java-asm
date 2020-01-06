using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes
{
    public class StackMapTableAttribute : CustomAttribute
    {
        public enum VerificationElementType
        {
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

        public abstract class VerificationElement
        {
            public abstract VerificationElementType Type { get; }
        }

        public class SimpleVerificationElement : VerificationElement
        {
            public override VerificationElementType Type { get; }

            public SimpleVerificationElement(VerificationElementType type)
            {
                type.CheckInAndThrow(nameof(type), VerificationElementType.Top, VerificationElementType.Integer,
                    VerificationElementType.Float, VerificationElementType.Long, VerificationElementType.Double, VerificationElementType.Null, VerificationElementType.UnitializedThis);
                Type = type;
            }
        }

        public class ObjectVerificationElement : VerificationElement
        {
            public override VerificationElementType Type => VerificationElementType.Object;

            public ClassName ObjectClass { get; set; }
        }

        public class UninitializedVerificationElement : VerificationElement
        {
            public override VerificationElementType Type => VerificationElementType.Unitialized;

            public ushort NewInstructionOffset { get; set; }
        }

        public enum FrameType
        {
            Same,
            SameLocals1StackItem,
            Chop,
            Append,
            Full
        }

        public class StackMapFrame
        {
            public FrameType Type { get; set; }

            public ushort OffsetDelta { get; set; }

            public List<VerificationElement> Stack { get; } = new List<VerificationElement>();

            public List<VerificationElement> Locals { get; } = new List<VerificationElement>();

            public int? ChopK { get; set; }
        }

        public List<StackMapFrame> Entries { get; } = new List<StackMapFrame>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            throw new NotImplementedException();
        }
    }

    internal class StackMapTableAttributeFactory : ICustomAttributeFactory<StackMapTableAttribute>
    {
        private static StackMapTableAttribute.VerificationElement ReadVerificationElement(Stream stream, ClassReaderState readerState)
        {
            var verificationElementType = (StackMapTableAttribute.VerificationElementType) stream.ReadByteFully();
            return verificationElementType switch
            {
                StackMapTableAttribute.VerificationElementType.Top => (StackMapTableAttribute.VerificationElement) new StackMapTableAttribute.SimpleVerificationElement(verificationElementType),
                StackMapTableAttribute.VerificationElementType.Integer => new StackMapTableAttribute.SimpleVerificationElement(verificationElementType),
                StackMapTableAttribute.VerificationElementType.Float => new StackMapTableAttribute.SimpleVerificationElement(verificationElementType),
                StackMapTableAttribute.VerificationElementType.Long => new StackMapTableAttribute.SimpleVerificationElement(verificationElementType),
                StackMapTableAttribute.VerificationElementType.Double => new StackMapTableAttribute.SimpleVerificationElement(verificationElementType),
                StackMapTableAttribute.VerificationElementType.Null => new StackMapTableAttribute.SimpleVerificationElement(verificationElementType),
                StackMapTableAttribute.VerificationElementType.UnitializedThis => new StackMapTableAttribute.SimpleVerificationElement(verificationElementType),
                StackMapTableAttribute.VerificationElementType.Object => new StackMapTableAttribute.ObjectVerificationElement
                {
                    ObjectClass = new ClassName(readerState.ConstantPool
                        .GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(stream))
                        .Name.String)
                },
                StackMapTableAttribute.VerificationElementType.Unitialized => new StackMapTableAttribute.UninitializedVerificationElement
                {
                    NewInstructionOffset = Binary.BigEndian.ReadUInt16(stream)
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public StackMapTableAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new StackMapTableAttribute();

            var parametersCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Entries.Capacity = parametersCount;
            for (var i = 0; i < parametersCount; i++)
            {
                StackMapTableAttribute.StackMapFrame stackMapFrame;
                var frameTypeByte = attributeDataStream.ReadByteFully();
                if (frameTypeByte < 64)
                {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame
                    {
                        Type = StackMapTableAttribute.FrameType.Same,
                        OffsetDelta = frameTypeByte
                    };
                } 
                else if (frameTypeByte < 128)
                {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame
                    {
                        Type = StackMapTableAttribute.FrameType.SameLocals1StackItem,
                        OffsetDelta = frameTypeByte
                    };

                    stackMapFrame.Stack.Add(ReadVerificationElement(attributeDataStream, readerState));
                }
                else if (frameTypeByte == 247)
                {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame
                    {
                        Type = StackMapTableAttribute.FrameType.SameLocals1StackItem,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };

                    stackMapFrame.Stack.Add(ReadVerificationElement(attributeDataStream, readerState));
                }
                else if (frameTypeByte < 251)
                {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame
                    {
                        Type = StackMapTableAttribute.FrameType.Chop,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream),
                        ChopK = 251 - frameTypeByte
                    };
                }
                else if (frameTypeByte == 251)
                {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame
                    {
                        Type = StackMapTableAttribute.FrameType.Same,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };
                }
                else if (frameTypeByte < 255)
                {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame
                    {
                        Type = StackMapTableAttribute.FrameType.Append,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };

                    for (var j = 0; j < frameTypeByte - 251; j++)
                        stackMapFrame.Locals.Add(ReadVerificationElement(attributeDataStream, readerState));
                }
                else if (frameTypeByte == 255)
                {
                    stackMapFrame = new StackMapTableAttribute.StackMapFrame
                    {
                        Type = StackMapTableAttribute.FrameType.Full,
                        OffsetDelta = Binary.BigEndian.ReadUInt16(attributeDataStream)
                    };

                    var localsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
                    for (var j = 0; j < localsCount; j++)
                        stackMapFrame.Locals.Add(ReadVerificationElement(attributeDataStream, readerState));
                    var stackCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
                    for (var j = 0; j < stackCount; j++)
                        stackMapFrame.Locals.Add(ReadVerificationElement(attributeDataStream, readerState));
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(frameTypeByte));

                attribute.Entries.Add(stackMapFrame);
            }

            return attribute;
        }
    }
}
