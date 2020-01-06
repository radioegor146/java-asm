using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes.TypeAnnotation;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes
{
    public class RuntimeInvisibleTypeAnnotationsAttribute : CustomAttribute
    {
        public List<TypeAnnotationNode> Annotations { get; set; } = new List<TypeAnnotationNode>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Annotations.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(Annotations.Count), $"Number of annotations is too big: {Annotations.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort)Annotations.Count);
            foreach (var annotation in Annotations)
                annotation.Write(attributeDataStream, writerState, scope);

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeInvisibleTypeAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeInvisibleTypeAnnotationsAttribute>
    {
        public RuntimeInvisibleTypeAnnotationsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new RuntimeInvisibleTypeAnnotationsAttribute();

            var annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Annotations.Capacity = annotationsCount;
            for (var i = 0; i < annotationsCount; i++)
                attribute.Annotations.Add(TypeAnnotationNode.Parse(attributeDataStream, readerState, scope));

            return attribute;
        }
    }
}
