using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.IO {
    internal class ConstantPool {
        private readonly Dictionary<Entry, ushort> constantPoolMap = new Dictionary<Entry, ushort>();
        private readonly List<Entry> entries = new List<Entry>();

        public ushort Find(Entry entry) {
            if (this.constantPoolMap.ContainsKey(entry)) {
                return this.constantPoolMap[entry];
            }

            entry.PutToConstantPool(this);
            this.entries.Add(entry);
            ushort newKey = (ushort) this.entries.Count;
            if (entry is LongEntry || entry is DoubleEntry)
                this.entries.Add(new LongDoublePlaceholderEntry());
            if (this.entries.Count > ushort.MaxValue)
                throw new Exception("Too much entries in constant pool");
            this.constantPoolMap.Add(entry, newKey);
            return newKey;
        }

        public T GetEntry<T>(ushort id) where T : Entry {
            return (T) this.entries[id - 1];
        }

        public void Read(Stream stream) {
            ushort size = Binary.BigEndian.ReadUInt16(stream);
            for (int i = 0; i < size - 1; i++) {
                EntryTag tag = (EntryTag) stream.ReadByteFully();
                Entry entry;
                switch (tag) {
                    case EntryTag.Class:                    entry = new ClassEntry(stream); break;
                    case EntryTag.FieldReference:           entry = new FieldReferenceEntry(stream); break;
                    case EntryTag.MethodReference:          entry = new MethodReferenceEntry(stream); break;
                    case EntryTag.InterfaceMethodReference: entry = new InterfaceMethodReferenceEntry(stream); break;
                    case EntryTag.String:                   entry = new StringEntry(stream); break;
                    case EntryTag.Integer:                  entry = new IntegerEntry(stream); break;
                    case EntryTag.Float:                    entry = new FloatEntry(stream); break;
                    case EntryTag.Long:                     entry = new LongEntry(stream); break;
                    case EntryTag.Double:                   entry = new DoubleEntry(stream); break;
                    case EntryTag.NameAndType:              entry = new NameAndTypeEntry(stream); break;
                    case EntryTag.Utf8:                     entry = new Utf8Entry(stream); break;
                    case EntryTag.MethodHandle:             entry = new MethodHandleEntry(stream); break;
                    case EntryTag.MethodType:               entry = new MethodTypeEntry(stream); break;
                    case EntryTag.InvokeDynamic:            entry = new InvokeDynamicEntry(stream); break;
                    default: throw new ArgumentOutOfRangeException(nameof(tag));
                }


                Debug.Assert(entry.Tag == tag);
                this.entries.Add(entry);
                if (!(entry is LongEntry) && !(entry is DoubleEntry))
                    continue;
                this.entries.Add(new LongDoublePlaceholderEntry());
                i++;
            }

            foreach (Entry entry in this.entries)
                entry.ProcessFromConstantPool(this);
        }

        private class LongDoublePlaceholderEntry : Entry {
            public override EntryTag Tag => throw new Exception("You shouldn't access that entry");

            public override void ProcessFromConstantPool(ConstantPool constantPool) {
            }

            public override void Write(Stream stream) {
            }

            public override void PutToConstantPool(ConstantPool constantPool) {
            }
        }

        public void Write(Stream stream) {
            if (this.entries.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.entries.Count),
                    $"Too many entries: {this.entries.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort) (this.entries.Count + 1));
            foreach (Entry entry in this.entries.Where(entry => !(entry is LongDoublePlaceholderEntry))) {
                stream.WriteByte((byte) entry.Tag);
                entry.Write(stream);
            }
        }
    }
}