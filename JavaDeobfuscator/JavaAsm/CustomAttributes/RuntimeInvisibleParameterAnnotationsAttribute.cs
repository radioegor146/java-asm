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

    internal class ParameterAnnotations
    {
        public List<AnnotationNode> Annotations { get; set; }
    }

    internal class RuntimeInvisibleParameterAnnotationsAttribute : CustomAttribute
    {

        public List<ParameterAnnotations> Parameters { get; set; } = new List<ParameterAnnotations>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Parameters.Count > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"Number of parameters is too big: {Parameters.Count} > {byte.MaxValue}");
            attributeDataStream.WriteByte((byte)Parameters.Count);
            foreach (var parameter in Parameters)
            {
                if (parameter.Annotations.Count > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(
                        $"Number of annotations is too big: {parameter.Annotations.Count} > {ushort.MaxValue}");
                Binary.BigEndian.Write(attributeDataStream, (ushort)parameter.Annotations.Count);
                foreach (var annotation in parameter.Annotations)
                    annotation.Write(attributeDataStream, writerState);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class RuntimeInvisibleParameterAnnotationsAttributeFactory : ICustomAttributeFactory<RuntimeInvisibleParameterAnnotationsAttribute>
    {
        public RuntimeInvisibleParameterAnnotationsAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var attribute = new RuntimeInvisibleParameterAnnotationsAttribute();

            var parametersCount = (byte)attributeDataStream.ReadByte();
            attribute.Parameters.Capacity = parametersCount;
            for (var i = 0; i < parametersCount; i++)
            {
                var parameter = new ParameterAnnotations();
                var annotationsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
                parameter.Annotations.Capacity = annotationsCount;
                for (var j = 0; j < annotationsCount; j++)
                    parameter.Annotations.Add(AnnotationNode.Parse(attributeDataStream, readerState));
                attribute.Parameters.Add(parameter);
            }

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for RuntimeVisibleParameterAnnotations attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
