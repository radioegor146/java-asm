using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation {
    public class LocalvarTarget : TypeAnnotationTarget {
        public class TableEntry {
            public ushort StartPc { get; set; }

            public ushort Length { get; set; }

            public ushort Index { get; set; }
        }

        public List<TableEntry> Table { get; } = new List<TableEntry>();

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.Localvar;

        internal override void Write(Stream stream, ClassWriterState writerState) {
            if (this.Table.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Table.Count), $"Table is too big: {this.Table.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort) this.Table.Count);
            foreach (TableEntry entry in this.Table) {
                Binary.BigEndian.Write(stream, entry.StartPc);
                Binary.BigEndian.Write(stream, entry.Length);
                Binary.BigEndian.Write(stream, entry.Index);
            }
        }

        internal override void Read(Stream stream, ClassReaderState readerState) {
            ushort tableSize = Binary.BigEndian.ReadUInt16(stream);
            this.Table.Capacity = tableSize;
            for (int i = 0; i < tableSize; i++) {
                this.Table.Add(new TableEntry {
                    StartPc = Binary.BigEndian.ReadUInt16(stream),
                    Length = Binary.BigEndian.ReadUInt16(stream),
                    Index = Binary.BigEndian.ReadUInt16(stream)
                });
            }
        }
    }
}