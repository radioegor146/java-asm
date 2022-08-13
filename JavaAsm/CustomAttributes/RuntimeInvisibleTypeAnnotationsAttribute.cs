using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes.TypeAnnotation;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes {
    public class RuntimeInvisibleTypeAnnotationsAttribute : CustomAttribute {
        public List<TypeAnnotationNode> Annotations { get; set; } = new List<TypeAnnotationNode>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.Annotations.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Annotations.Count), $"Number of annotations is too big: {this.Annotations.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.Annotations.Count);
            foreach (TypeAnnotationNode annotation in this.Annotations)
                annotation.Write(attributeDataStream, writerState, scope);

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeInvisibleTypeAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeInvisibleTypeAnnotationsAttribute> {
        public RuntimeInvisibleTypeAnnotationsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            RuntimeInvisibleTypeAnnotationsAttribute attribute = new RuntimeInvisibleTypeAnnotationsAttribute();

            ushort annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Annotations.Capacity = annotationsCount;
            for (int i = 0; i < annotationsCount; i++)
                attribute.Annotations.Add(TypeAnnotationNode.Parse(attributeDataStream, readerState, scope));

            return attribute;
        }
    }
}