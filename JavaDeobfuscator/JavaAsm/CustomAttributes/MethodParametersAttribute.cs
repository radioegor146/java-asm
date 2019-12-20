using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class MethodParametersAttribute : CustomAttribute
    {
        public class Parameter
        {
            public string Name { get; set; }

            public AccessModifiers Access { get; set; }
        }

        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Parameters.Count > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"Line number table is too big: {Parameters.Count} > {byte.MaxValue}");
            attributeDataStream.WriteByte((byte) Parameters.Count);
            foreach (var parameter in Parameters)
            {
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new Utf8Entry(parameter.Name)));
                Binary.BigEndian.Write(attributeDataStream, (ushort) parameter.Access);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class MethodParametersAttributeFactory : ICustomAttributeFactory<MethodParametersAttribute>
    {
        public MethodParametersAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var attribute = new MethodParametersAttribute();

            var exceptionTableSize = (byte) attributeDataStream.ReadByte();
            attribute.Parameters.Capacity = exceptionTableSize;
            for (var i = 0; i < exceptionTableSize; i++)
                attribute.Parameters.Add(new MethodParametersAttribute.Parameter
                {
                    Name = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String,
                    Access = (AccessModifiers) Binary.BigEndian.ReadUInt16(attributeDataStream)
                });

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for MethodParameters attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
