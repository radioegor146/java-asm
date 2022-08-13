using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes {
    public class LocalVariableTableAttribute : CustomAttribute {
        public class LocalVariableTableEntry {
            public ushort StartPc { get; set; }

            public ushort Length { get; set; }

            public string Name { get; set; }

            public TypeDescriptor Descriptor { get; set; }

            public ushort Index { get; set; }

            public uint EndPC => (uint) this.StartPc + (uint) this.Length;
        }

        public List<LocalVariableTableEntry> LocalVariableTable { get; set; } = new List<LocalVariableTableEntry>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.LocalVariableTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.LocalVariableTable.Count), $"Local variable table is too big: {this.LocalVariableTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.LocalVariableTable.Count);
            foreach (LocalVariableTableEntry localVariableTableEntry in this.LocalVariableTable) {
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

    internal class LocalVariableTableAttributeFactory : ICustomAttributeFactory<LocalVariableTableAttribute> {
        public LocalVariableTableAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            LocalVariableTableAttribute attribute = new LocalVariableTableAttribute();

            ushort localVariableTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.LocalVariableTable.Capacity = localVariableTableSize;
            for (int i = 0; i < localVariableTableSize; i++)
                attribute.LocalVariableTable.Add(new LocalVariableTableAttribute.LocalVariableTableEntry {
                    StartPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    Length = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    Name = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String,
                    Descriptor = TypeDescriptor.Parse(readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String),
                    Index = Binary.BigEndian.ReadUInt16(attributeDataStream)
                });

            return attribute;
        }
    }
}