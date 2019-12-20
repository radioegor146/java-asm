using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class LocalVariableTypeTableAttribute : CustomAttribute
    {
        public class LocalVariableTypeTableEntry
        {
            public ushort StartPc { get; set; }

            public ushort Length { get; set; }
            
            public string Name { get; set; }

            public string Signature { get; set; }

            public ushort Index { get; set; }
        }

        public List<LocalVariableTypeTableEntry> LocalVariableTypeTable { get; set; } = new List<LocalVariableTypeTableEntry>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (LocalVariableTypeTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Local variable table is too big: {LocalVariableTypeTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort)LocalVariableTypeTable.Count);
            foreach (var localVariableTypeTableEntry in LocalVariableTypeTable)
            {
                Binary.BigEndian.Write(attributeDataStream, localVariableTypeTableEntry.StartPc);
                Binary.BigEndian.Write(attributeDataStream, localVariableTypeTableEntry.Length);
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new Utf8Entry(localVariableTypeTableEntry.Name)));
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new Utf8Entry(localVariableTypeTableEntry.Signature)));
                Binary.BigEndian.Write(attributeDataStream, localVariableTypeTableEntry.Index);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class LocalVariableTypeTableAttributeFactory : ICustomAttributeFactory<LocalVariableTypeTableAttribute>
    {
        public LocalVariableTypeTableAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var attribute = new LocalVariableTypeTableAttribute();

            var localVariableTypeTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.LocalVariableTypeTable.Capacity = localVariableTypeTableSize;
            for (var i = 0; i < localVariableTypeTableSize; i++)
                attribute.LocalVariableTypeTable.Add(new LocalVariableTypeTableAttribute.LocalVariableTypeTableEntry
                {
                    StartPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    Length = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    Name = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String,
                    Signature = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String,
                    Index = Binary.BigEndian.ReadUInt16(attributeDataStream)
                });

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for LocalVariableTypeTable attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
