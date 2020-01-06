using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class LineNumberTableAttribute : CustomAttribute
    {
        public class LineNumberTableEntry
        {
            public ushort StartPc { get; set; }

            public ushort LineNumber { get; set; }
        }

        public List<LineNumberTableEntry> LineNumberTable { get; set; } = new List<LineNumberTableEntry>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (LineNumberTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Line number table too big: {LineNumberTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort)LineNumberTable.Count);
            foreach (var exceptionTableEntry in LineNumberTable)
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
            var attribute = new LineNumberTableAttribute();

            var exceptionTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.LineNumberTable.Capacity = exceptionTableSize;
            for (var i = 0; i < exceptionTableSize; i++)
                attribute.LineNumberTable.Add(new LineNumberTableAttribute.LineNumberTableEntry
                {
                    StartPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    LineNumber = Binary.BigEndian.ReadUInt16(attributeDataStream)
                });

            return attribute;
        }
    }
}
