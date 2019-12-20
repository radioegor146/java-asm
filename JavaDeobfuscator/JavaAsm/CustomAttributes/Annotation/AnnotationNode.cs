using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation
{
    internal class AnnotationNode
    {
        public TypeDescriptor Type { get; set; }

        public class ElementValuePair
        {
            public string ElementName { get; set; }

            public ElementValue Value { get; set; }
        }

        public List<ElementValuePair> ElementValuePairs { get; set; } = new List<ElementValuePair>();

        public static AnnotationNode Parse(Stream stream, ClassReaderState readerState)
        {
            var annotation = new AnnotationNode
            {
                Type = TypeDescriptor.Parse(readerState.ConstantPool
                    .GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String)
            };
            var elementValuePairsCount = Binary.BigEndian.ReadUInt16(stream);
            annotation.ElementValuePairs.Capacity = elementValuePairsCount;
            for (var i = 0; i < elementValuePairsCount; i++)
                annotation.ElementValuePairs.Add(new ElementValuePair
                {
                    ElementName = readerState.ConstantPool
                        .GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String,
                    Value = ElementValue.Parse(stream, readerState)
                });
            return annotation;
        }

        public void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, writerState.ConstantPool.Find(new Utf8Entry(Type.ToString())));
            if (ElementValuePairs.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(
                    $"Too many ElementValues: {ElementValuePairs.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort) ElementValuePairs.Count);
            foreach (var elementValuePair in ElementValuePairs)
            {
                Binary.BigEndian.Write(stream,
                    writerState.ConstantPool.Find(new Utf8Entry(elementValuePair.ElementName)));
                elementValuePair.Value.Write(stream, writerState);
            }
        }
    }
}
