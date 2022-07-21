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
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.ExceptionTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.ExceptionTable.Count),
                    $"Exception table size too big: {this.ExceptionTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.ExceptionTable.Count);
            foreach (ClassName exceptionClassName in this.ExceptionTable)
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(exceptionClassName.Name))));

            return attributeDataStream.ToArray();
        }
    }

    internal class ExceptionsAttributeFactory : ICustomAttributeFactory<ExceptionsAttribute>
    {
        public ExceptionsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            ExceptionsAttribute attribute = new ExceptionsAttribute();

            ushort count = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.ExceptionTable.Capacity = count;
            for (int i = 0; i < count; i++)
                attribute.ExceptionTable.Add(new ClassName(readerState.ConstantPool
                    .GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).Name.String));

            return attribute;
        }
    }
}
