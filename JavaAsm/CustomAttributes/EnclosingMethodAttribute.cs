﻿using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes
{
    public class EnclosingMethodAttribute : CustomAttribute
    {
        public ClassName Class { get; set; }

        public string MethodName { get; set; }
        public MethodDescriptor MethodDescriptor { get; set; }

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            Binary.BigEndian.Write(attributeDataStream,
                writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(Class.Name))));

            if (MethodName == null && MethodDescriptor == null)
            {
                Binary.BigEndian.Write(attributeDataStream, (ushort) 0);
            } 
            else
            {
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new NameAndTypeEntry(new Utf8Entry(MethodName),
                        new Utf8Entry(MethodDescriptor.ToString()))));
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class EnclosingMethodAttributeFactory : ICustomAttributeFactory<EnclosingMethodAttribute>
    {
        public EnclosingMethodAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new EnclosingMethodAttribute
            {
                Class = new ClassName(readerState.ConstantPool
                    .GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).Name.String)
            };

            var nameAndTypeEntryIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);

            if (nameAndTypeEntryIndex == 0) 
                return attribute;

            var nameAndTypeEntry = readerState.ConstantPool.GetEntry<NameAndTypeEntry>(nameAndTypeEntryIndex);
            attribute.MethodName = nameAndTypeEntry.Name.String;
            attribute.MethodDescriptor = MethodDescriptor.Parse(nameAndTypeEntry.Descriptor.String);

            return attribute;
        }
    }
}