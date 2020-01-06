using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes
{
    public class RuntimeVisibleAnnotationsAttribute : CustomAttribute
    {
        public List<AnnotationNode> Annotations { get; set; } = new List<AnnotationNode>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Annotations.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(Annotations.Count), $"Number of annotations is too big: {Annotations.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort)Annotations.Count);
            foreach (var annotation in Annotations)
                annotation.Write(attributeDataStream, writerState);

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeVisibleAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeVisibleAnnotationsAttribute>
    {
        public RuntimeVisibleAnnotationsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new RuntimeVisibleAnnotationsAttribute();

            var annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Annotations.Capacity = annotationsCount;
            for (var i = 0; i < annotationsCount; i++)
                attribute.Annotations.Add(AnnotationNode.Parse(attributeDataStream, readerState));

            return attribute;
        }
    }
}
