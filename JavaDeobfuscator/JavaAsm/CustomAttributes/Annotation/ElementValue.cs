using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation
{
    internal class ElementValue
    {
        public enum ElementValueTag
        {
            Byte = 'B',
            Character = 'C',
            Double = 'D',
            Float = 'F',
            Integer = 'I',
            Long = 'J',
            Short = 'S',
            Boolean = 'Z',
            String = 's',
            Enum = 'e',
            Class = 'c',
            Annotation = '@',
            Array = '['
        };

        public ElementValueTag Tag { get; set; }

        public object ConstValue { get; set; }

        public class EnumConstValueType
        {
            public TypeDescriptor TypeName { get; set; }

            public string ConstName { get; set; }
        }

        public EnumConstValueType EnumConstValue { get; set; }

        public TypeDescriptor Class { get; set; }

        public AnnotationNode AnnotationNode { get; set; }

        public List<ElementValue> ArrayValue { get; set; }

        public static ElementValue Parse(Stream stream, ClassReaderState readerState)
        {
            var elementValue = new ElementValue
            {
                Tag = (ElementValueTag) stream.ReadByte()
            };

            switch (elementValue.Tag)
            {
                case ElementValueTag.Byte:
                case ElementValueTag.Character:
                case ElementValueTag.Integer:
                case ElementValueTag.Short:
                case ElementValueTag.Boolean:
                    elementValue.ConstValue = readerState.ConstantPool.GetEntry<IntegerEntry>(Binary.BigEndian.ReadUInt16(stream)).Value;
                    break;
                case ElementValueTag.Double:
                    elementValue.ConstValue = readerState.ConstantPool.GetEntry<DoubleEntry>(Binary.BigEndian.ReadUInt16(stream)).Value;
                    break;
                case ElementValueTag.Float:
                    elementValue.ConstValue = readerState.ConstantPool.GetEntry<FloatEntry>(Binary.BigEndian.ReadUInt16(stream)).Value;
                    break;
                case ElementValueTag.Long:
                    elementValue.ConstValue = readerState.ConstantPool.GetEntry<LongEntry>(Binary.BigEndian.ReadUInt16(stream)).Value;
                    break;
                case ElementValueTag.String:
                    elementValue.ConstValue = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String;
                    break;
                case ElementValueTag.Enum:
                    elementValue.EnumConstValue = new EnumConstValueType
                    {
                        TypeName = TypeDescriptor.Parse(readerState.ConstantPool
                            .GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String),
                        ConstName = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream))
                            .String
                    };
                    break;
                case ElementValueTag.Class:
                    elementValue.Class = TypeDescriptor.Parse(readerState.ConstantPool
                        .GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String, true);
                    break;
                case ElementValueTag.Annotation:
                    elementValue.AnnotationNode = AnnotationNode.Parse(stream, readerState);
                    break;
                case ElementValueTag.Array:
                    var arraySize = Binary.BigEndian.ReadUInt16(stream);
                    elementValue.ArrayValue = new List<ElementValue>(arraySize);
                    for (var i = 0; i < arraySize; i++)
                        elementValue.ArrayValue.Add(Parse(stream, readerState));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(elementValue.Tag));
            }

            return elementValue;
        }

        public void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte((byte) Tag);
            switch (Tag)
            {
                case ElementValueTag.Byte:
                case ElementValueTag.Character:
                case ElementValueTag.Integer:
                case ElementValueTag.Short:
                case ElementValueTag.Boolean:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new IntegerEntry((int) ConstValue)));
                    break;
                case ElementValueTag.Double:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new DoubleEntry((double) ConstValue)));
                    break;
                case ElementValueTag.Float:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new FloatEntry((float) ConstValue)));
                    break;
                case ElementValueTag.Long:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new LongEntry((long) ConstValue)));
                    break;
                case ElementValueTag.String:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new StringEntry(new Utf8Entry((string) ConstValue))));
                    break;
                case ElementValueTag.Enum:
                    Binary.BigEndian.Write(stream,
                        writerState.ConstantPool.Find(new Utf8Entry(EnumConstValue.TypeName.ToString())));
                    Binary.BigEndian.Write(stream,
                        writerState.ConstantPool.Find(new Utf8Entry(EnumConstValue.ConstName)));
                    break;
                case ElementValueTag.Class:
                    Binary.BigEndian.Write(stream,
                        writerState.ConstantPool.Find(new Utf8Entry(Class.ToString())));
                    break;
                case ElementValueTag.Annotation:
                    AnnotationNode.Write(stream, writerState);
                    break;
                case ElementValueTag.Array:
                    if (ArrayValue.Count > ushort.MaxValue)
                        throw new ArgumentException($"Array size is too big: {ArrayValue.Count} > {ushort.MaxValue}");
                    Binary.BigEndian.Write(stream, (ushort) ArrayValue.Count);
                    foreach (var element in ArrayValue)
                        element.Write(stream, writerState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Tag));
            }
        }
    }
}
