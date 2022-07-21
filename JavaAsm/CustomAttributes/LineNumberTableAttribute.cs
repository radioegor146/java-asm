using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes
{
    public class LineNumberTableAttribute : CustomAttribute
    {
        public class LineNumberTableEntry
        {
            public ushort StartPc { get; set; }

            public ushort LineNumber { get; set; }
        }

        public List<LineNumberTableEntry> LineNumberTable { get; set; } = new List<LineNumberTableEntry>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.LineNumberTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.LineNumberTable.Count), $"Line number table too big: {this.LineNumberTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.LineNumberTable.Count);
            foreach (LineNumberTableEntry exceptionTableEntry in this.LineNumberTable)
            {
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.StartPc);
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.LineNumber);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class LineNumberTableAttributeFactory : ICustomAttributeFactory<LineNumberTableAttribute>
    {
        public LineNumberTableAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            LineNumberTableAttribute attribute = new LineNumberTableAttribute();

            ushort exceptionTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.LineNumberTable.Capacity = exceptionTableSize;
            for (int i = 0; i < exceptionTableSize; i++)
                attribute.LineNumberTable.Add(new LineNumberTableAttribute.LineNumberTableEntry
                {
                    StartPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    LineNumber = Binary.BigEndian.ReadUInt16(attributeDataStream)
                });

            return attribute;
        }
    }
}
