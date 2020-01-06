using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes
{
    public class LocalVariableTypeTableAttribute : CustomAttribute
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

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
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
        public LocalVariableTypeTableAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
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

            return attribute;
        }
    }
}
