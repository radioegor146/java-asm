using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes
{
    public class MethodParametersAttribute : CustomAttribute
    {
        public class Parameter
        {
            public string Name { get; set; }

            public ClassAccessModifiers Access { get; set; }
        }

        public List<Parameter> Parameters { get; set; } = new List<Parameter>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
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
        public MethodParametersAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new MethodParametersAttribute();

            var exceptionTableSize = attributeDataStream.ReadByteFully();
            attribute.Parameters.Capacity = exceptionTableSize;
            for (var i = 0; i < exceptionTableSize; i++)
                attribute.Parameters.Add(new MethodParametersAttribute.Parameter
                {
                    Name = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String,
                    Access = (ClassAccessModifiers) Binary.BigEndian.ReadUInt16(attributeDataStream)
                });

            return attribute;
        }
    }
}
