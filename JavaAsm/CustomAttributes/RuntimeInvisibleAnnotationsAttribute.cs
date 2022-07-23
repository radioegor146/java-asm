using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes {
    public class RuntimeInvisibleAnnotationsAttribute : CustomAttribute {
        public List<AnnotationNode> Annotations { get; set; } = new List<AnnotationNode>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.Annotations.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Annotations.Count), $"Number of annotations is too big: {this.Annotations.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.Annotations.Count);
            foreach (AnnotationNode annotation in this.Annotations)
                annotation.Write(attributeDataStream, writerState);

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeInvisibleAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeInvisibleAnnotationsAttribute> {
        public RuntimeInvisibleAnnotationsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            RuntimeInvisibleAnnotationsAttribute attribute = new RuntimeInvisibleAnnotationsAttribute();

            ushort annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Annotations.Capacity = annotationsCount;
            for (int i = 0; i < annotationsCount; i++)
                attribute.Annotations.Add(AnnotationNode.Parse(attributeDataStream, readerState));

            return attribute;
        }
    }
}