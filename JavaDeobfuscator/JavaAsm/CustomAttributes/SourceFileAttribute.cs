using System;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class SourceFileAttribute : CustomAttribute
    {
        public string Value { get; set; }

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            Binary.BigEndian.Write(attributeDataStream, writerState.ConstantPool.Find(new Utf8Entry(Value)));

            return attributeDataStream.ToArray();
        }
    }

    internal class SourceFileAttributeFactory : ICustomAttributeFactory<SourceFileAttribute>
    {
        public SourceFileAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            return new SourceFileAttribute 
            {
                Value = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String
            };
        }
    }
}
