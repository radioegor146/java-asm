using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class EnclosingMethodAttribute : CustomAttribute
    {
        public ClassName Class { get; set; }

        public string MethodName { get; set; }
        public MethodDescriptor MethodDescriptor { get; set; }

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            throw new NotImplementedException(); 
        }
    }

    internal class EnclosingMethodAttributeFactory : ICustomAttributeFactory<EnclosingMethodAttribute>
    {
        public EnclosingMethodAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            if (attributeNode.Data.Length != sizeof(ushort) * 2)
                throw new ArgumentOutOfRangeException($"Attribute length is incorrect for EnclosingMethod: {attributeNode.Data.Length} != {sizeof(ushort) * 2}");

            using var attributeDataStream = new MemoryStream(attributeNode.Data);

            var attribute = new EnclosingMethodAttribute
            {
                Class = new ClassName(readerState.ConstantPool
                    .GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).Name.String)
            };

            var nameAndTypeEntryIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);

            if (nameAndTypeEntryIndex != 0)
            {
                var nameAndTypeEntry = readerState.ConstantPool.GetEntry<NameAndTypeEntry>(nameAndTypeEntryIndex);
                attribute.MethodName = nameAndTypeEntry.Name.String;
                attribute.MethodDescriptor = MethodDescriptor.Parse(nameAndTypeEntry.Descriptor.String);
            }

            return attribute;
        }
    }
}
