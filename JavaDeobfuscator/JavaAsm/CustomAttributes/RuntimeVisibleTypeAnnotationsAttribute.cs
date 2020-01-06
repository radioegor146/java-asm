using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation;
using JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class RuntimeVisibleTypeAnnotationsAttribute : CustomAttribute
    {
        public List<TypeAnnotationNode> Annotations { get; set; } = new List<TypeAnnotationNode>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Annotations.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Number of annotations is too big: {Annotations.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort)Annotations.Count);
            foreach (var annotation in Annotations)
                annotation.Write(attributeDataStream, writerState, scope);

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeVisibleTypeAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeVisibleTypeAnnotationsAttribute>
    {
        public RuntimeVisibleTypeAnnotationsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new RuntimeVisibleTypeAnnotationsAttribute();

            var annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Annotations.Capacity = annotationsCount;
            for (var i = 0; i < annotationsCount; i++)
                attribute.Annotations.Add(TypeAnnotationNode.Parse(attributeDataStream, readerState, scope));

            return attribute;
        }
    }
}
