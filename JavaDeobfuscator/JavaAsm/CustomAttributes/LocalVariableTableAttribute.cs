using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class LocalVariableTableAttribute : CustomAttribute
    {
        public class LocalVariableTableEntry
        {
            public ushort StartPc { get; set; }

            public ushort Length { get; set; }

            public string Name { get; set; }

            public TypeDescriptor Descriptor { get; set; }

            public ushort Index { get; set; }
        }

        public List<LocalVariableTableEntry> LocalVariableTable { get; set; } = new List<LocalVariableTableEntry>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (LocalVariableTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Local variable table is too big: {LocalVariableTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) LocalVariableTable.Count);
            foreach (var localVariableTableEntry in LocalVariableTable)
            {
                Binary.BigEndian.Write(attributeDataStream, localVariableTableEntry.StartPc);
                Binary.BigEndian.Write(attributeDataStream, localVariableTableEntry.Length);
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new Utf8Entry(localVariableTableEntry.Name)));
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new Utf8Entry(localVariableTableEntry.Descriptor.ToString())));
                Binary.BigEndian.Write(attributeDataStream, localVariableTableEntry.Index);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class LocalVariableTableAttributeFactory : ICustomAttributeFactory<LocalVariableTableAttribute>
    {
        public LocalVariableTableAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var attribute = new LocalVariableTableAttribute();

            var localVariableTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.LocalVariableTable.Capacity = localVariableTableSize;
            for (var i = 0; i < localVariableTableSize; i++)
                attribute.LocalVariableTable.Add(new LocalVariableTableAttribute.LocalVariableTableEntry
                {
                    StartPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    Length = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    Name = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String,
                    Descriptor = TypeDescriptor.Parse(readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String),
                    Index = Binary.BigEndian.ReadUInt16(attributeDataStream)
                });

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for LocalVariableTable attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
