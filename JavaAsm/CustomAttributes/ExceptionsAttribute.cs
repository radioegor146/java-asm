using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes
{
    public class ExceptionsAttribute : CustomAttribute
    {
        public List<ClassName> ExceptionTable { get; set; } = new List<ClassName>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (ExceptionTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(
                    $"Exception table size too big: {ExceptionTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) ExceptionTable.Count);
            foreach (var exceptionClassName in ExceptionTable)
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(exceptionClassName.Name))));

            return attributeDataStream.ToArray();
        }
    }

    internal class ExceptionsAttributeFactory : ICustomAttributeFactory<ExceptionsAttribute>
    {
        public ExceptionsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new ExceptionsAttribute();

            var count = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.ExceptionTable.Capacity = count;
            for (var i = 0; i < count; i++)
                attribute.ExceptionTable.Add(new ClassName(readerState.ConstantPool
                    .GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).Name.String));

            return attribute;
        }
    }
}
