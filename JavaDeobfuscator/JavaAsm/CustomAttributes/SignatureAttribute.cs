using System;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class SignatureAttribute : CustomAttribute
    {
        public string Value { get; set; }

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            Binary.BigEndian.Write(attributeDataStream, writerState.ConstantPool.Find(new Utf8Entry(Value)));

            return attributeDataStream.ToArray();
        }
    }

    internal class SignatureAttributeFactory : ICustomAttributeFactory<SignatureAttribute>
    {
        public SignatureAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            if (attributeNode.Data.Length != sizeof(ushort))
                throw new ArgumentOutOfRangeException($"Attribute length is incorrect for Signature: {attributeNode.Data.Length} != {sizeof(ushort)}");
            return new SignatureAttribute
            {
                Value = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.GetUInt16(attributeNode.Data)).String
            };
        }
    }
}
