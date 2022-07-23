using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes {
    public class LocalVariableTypeTableAttribute : CustomAttribute {
        public class LocalVariableTypeTableEntry {
            public ushort StartPc { get; set; }

            public ushort Length { get; set; }

            public string Name { get; set; }

            public string Signature { get; set; }

            public ushort Index { get; set; }
        }

        public List<LocalVariableTypeTableEntry> LocalVariableTypeTable { get; set; } = new List<LocalVariableTypeTableEntry>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.LocalVariableTypeTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.LocalVariableTypeTable.Count), $"Local variable type table is too big: {this.LocalVariableTypeTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.LocalVariableTypeTable.Count);
            foreach (LocalVariableTypeTableEntry localVariableTypeTableEntry in this.LocalVariableTypeTable) {
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

    internal class LocalVariableTypeTableAttributeFactory : ICustomAttributeFactory<LocalVariableTypeTableAttribute> {
        public LocalVariableTypeTableAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            LocalVariableTypeTableAttribute attribute = new LocalVariableTypeTableAttribute();

            ushort localVariableTypeTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.LocalVariableTypeTable.Capacity = localVariableTypeTableSize;
            for (int i = 0; i < localVariableTypeTableSize; i++)
                attribute.LocalVariableTypeTable.Add(new LocalVariableTypeTableAttribute.LocalVariableTypeTableEntry {
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