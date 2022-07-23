using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.Helpers;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes {
    public class ParameterAnnotations {
        public List<AnnotationNode> Annotations { get; set; } = new List<AnnotationNode>();
    }

    public class RuntimeInvisibleParameterAnnotationsAttribute : CustomAttribute {
        public List<ParameterAnnotations> Parameters { get; set; } = new List<ParameterAnnotations>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.Parameters.Count > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Parameters.Count), $"Number of parameters is too big: {this.Parameters.Count} > {byte.MaxValue}");
            attributeDataStream.WriteByte((byte) this.Parameters.Count);
            foreach (ParameterAnnotations parameter in this.Parameters) {
                if (parameter.Annotations.Count > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(parameter.Annotations.Count),
                        $"Number of annotations is too big: {parameter.Annotations.Count} > {ushort.MaxValue}");
                Binary.BigEndian.Write(attributeDataStream, (ushort) parameter.Annotations.Count);
                foreach (AnnotationNode annotation in parameter.Annotations)
                    annotation.Write(attributeDataStream, writerState);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeInvisibleParameterAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeInvisibleParameterAnnotationsAttribute> {
        public RuntimeInvisibleParameterAnnotationsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            RuntimeInvisibleParameterAnnotationsAttribute attribute = new RuntimeInvisibleParameterAnnotationsAttribute();

            byte parametersCount = attributeDataStream.ReadByteFully();
            attribute.Parameters.Capacity = parametersCount;
            for (int i = 0; i < parametersCount; i++) {
                ParameterAnnotations parameter = new ParameterAnnotations();
                ushort annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
                parameter.Annotations.Capacity = annotationsCount;
                for (int j = 0; j < annotationsCount; j++)
                    parameter.Annotations.Add(AnnotationNode.Parse(attributeDataStream, readerState));
                attribute.Parameters.Add(parameter);
            }

            return attribute;
        }
    }
}