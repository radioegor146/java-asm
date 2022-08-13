using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes.Annotation {
    public class ElementValue {
        public enum ElementValueTag {
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

        private object constValue;

        public object ConstValue {
            get => this.constValue;
            set {
                this.constValue = value;
            }
        }

        public class EnumConstValueType {
            public TypeDescriptor TypeName { get; set; }

            public string ConstName { get; set; }
        }

        public EnumConstValueType EnumConstValue { get; set; }

        public TypeDescriptor Class { get; set; }

        public AnnotationNode AnnotationNode { get; set; }

        public List<ElementValue> ArrayValue { get; set; }

        internal static ElementValue Parse(Stream stream, ClassReaderState readerState) {
            ElementValue elementValue = new ElementValue {
                Tag = (ElementValueTag) stream.ReadByteFully()
            };

            switch (elementValue.Tag) {
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
                    elementValue.EnumConstValue = new EnumConstValueType {
                        TypeName = TypeDescriptor.Parse(readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String),
                        ConstName = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String
                    };
                    break;
                case ElementValueTag.Class:
                    elementValue.Class = TypeDescriptor.Parse(readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String, true);
                    break;
                case ElementValueTag.Annotation:
                    elementValue.AnnotationNode = AnnotationNode.Parse(stream, readerState);
                    break;
                case ElementValueTag.Array:
                    ushort arraySize = Binary.BigEndian.ReadUInt16(stream);
                    elementValue.ArrayValue = new List<ElementValue>(arraySize);
                    for (int i = 0; i < arraySize; i++)
                        elementValue.ArrayValue.Add(Parse(stream, readerState));
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(elementValue.Tag));
            }

            return elementValue;
        }

        internal void Write(Stream stream, ClassWriterState writerState) {
            stream.WriteByte((byte) this.Tag);
            switch (this.Tag) {
                case ElementValueTag.Byte:
                case ElementValueTag.Character:
                case ElementValueTag.Integer:
                case ElementValueTag.Short:
                case ElementValueTag.Boolean:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new IntegerEntry((int) this.ConstValue)));
                    break;
                case ElementValueTag.Double:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new DoubleEntry((double) this.ConstValue)));
                    break;
                case ElementValueTag.Float:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new FloatEntry((float) this.ConstValue)));
                    break;
                case ElementValueTag.Long:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new LongEntry((long) this.ConstValue)));
                    break;
                case ElementValueTag.String:
                    Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new Utf8Entry((string) this.ConstValue)));
                    break;
                case ElementValueTag.Enum:
                    Binary.BigEndian.Write(stream,
                        writerState.ConstantPool.Find(new Utf8Entry(this.EnumConstValue.TypeName.ToString())));
                    Binary.BigEndian.Write(stream,
                        writerState.ConstantPool.Find(new Utf8Entry(this.EnumConstValue.ConstName)));
                    break;
                case ElementValueTag.Class:
                    Binary.BigEndian.Write(stream,
                        writerState.ConstantPool.Find(new Utf8Entry(this.Class.ToString())));
                    break;
                case ElementValueTag.Annotation:
                    this.AnnotationNode.Write(stream, writerState);
                    break;
                case ElementValueTag.Array:
                    if (this.ArrayValue.Count > ushort.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(this.ArrayValue.Count), $"Array size is too big: {this.ArrayValue.Count} > {ushort.MaxValue}");
                    Binary.BigEndian.Write(stream, (ushort) this.ArrayValue.Count);
                    foreach (ElementValue element in this.ArrayValue)
                        element.Write(stream, writerState);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(this.Tag));
            }
        }
    }
}