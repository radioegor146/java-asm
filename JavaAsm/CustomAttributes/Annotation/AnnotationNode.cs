using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes.Annotation {
    public class AnnotationNode {
        public TypeDescriptor Type { get; set; }

        public class ElementValuePair {
            public string ElementName { get; set; }

            public ElementValue Value { get; set; }
        }

        public List<ElementValuePair> ElementValuePairs { get; set; } = new List<ElementValuePair>();

        internal static AnnotationNode Parse(Stream stream, ClassReaderState readerState) {
            AnnotationNode annotation = new AnnotationNode {
                Type = TypeDescriptor.Parse(readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String)
            };
            ushort elementValuePairsCount = Binary.BigEndian.ReadUInt16(stream);
            annotation.ElementValuePairs.Capacity = elementValuePairsCount;
            for (int i = 0; i < elementValuePairsCount; i++)
                annotation.ElementValuePairs.Add(new ElementValuePair {
                    ElementName = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String,
                    Value = ElementValue.Parse(stream, readerState)
                });
            return annotation;
        }

        internal void Write(Stream stream, ClassWriterState writerState) {
            Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new Utf8Entry(this.Type.ToString())));
            if (this.ElementValuePairs.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.ElementValuePairs.Count),
                    $"Too many ElementValues: {this.ElementValuePairs.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort) this.ElementValuePairs.Count);
            foreach (ElementValuePair elementValuePair in this.ElementValuePairs) {
                Binary.BigEndian.Write(stream,
                    writerState.ConstantPool.Find(new Utf8Entry(elementValuePair.ElementName)));
                elementValuePair.Value.Write(stream, writerState);
            }
        }
    }
}