using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class RuntimeVisibleAnnotationsAttribute : CustomAttribute
    {
        public List<AnnotationNode> Annotations { get; set; } = new List<AnnotationNode>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Annotations.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Number of annotations is too big: {Annotations.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort)Annotations.Count);
            foreach (var annotation in Annotations)
                annotation.Write(attributeDataStream, writerState);

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeVisibleAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeVisibleAnnotationsAttribute>
    {
        public RuntimeVisibleAnnotationsAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var attribute = new RuntimeVisibleAnnotationsAttribute();

            var annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Annotations.Capacity = annotationsCount;
            for (var i = 0; i < annotationsCount; i++)
                attribute.Annotations.Add(AnnotationNode.Parse(attributeDataStream, readerState));

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for RuntimeVisibleAnnotations attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
