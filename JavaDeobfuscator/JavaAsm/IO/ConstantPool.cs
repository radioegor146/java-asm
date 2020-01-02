using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.Helpers;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.IO
{
    public class ConstantPool
    {
        private readonly Dictionary<Entry, ushort> constantPoolMap = new Dictionary<Entry, ushort>();
        private readonly List<Entry> entries = new List<Entry>();

        public ushort Find(Entry entry)
        {
            if (constantPoolMap.ContainsKey(entry))
                return constantPoolMap[entry];
            entry.PutToConstantPool(this);
            entries.Add(entry);
            var newKey = (ushort)entries.Count;
            constantPoolMap.Add(entry, newKey);
            return newKey;
        }

        public T GetEntry<T>(ushort id) where T : Entry
        {
            return (T) entries[id - 1];
        }

        public void Read(Stream stream)
        {
            var size = Binary.BigEndian.ReadUInt16(stream);
            for (var i = 0; i < size - 1; i++)
            {
                var tag = (EntryTag) stream.ReadByte();
                var entry = tag switch
                {
                    EntryTag.Class => (Entry)new ClassEntry(stream),
                    EntryTag.FieldRef => new FieldReferenceEntry(stream),
                    EntryTag.MethodRef => new MethodReferenceEntry(stream),
                    EntryTag.InterfaceMethodRef => new InterfaceMethodReferenceEntry(stream),
                    EntryTag.String => new StringEntry(stream),
                    EntryTag.Integer => new IntegerEntry(stream),
                    EntryTag.Float => new FloatEntry(stream),
                    EntryTag.Long => new LongEntry(stream),
                    EntryTag.Double => new DoubleEntry(stream),
                    EntryTag.NameAndType => new NameAndTypeEntry(stream),
                    EntryTag.Utf8 => new Utf8Entry(stream),
                    EntryTag.MethodHandle => new MethodHandleEntry(stream),
                    EntryTag.MethodType => new MethodTypeEntry(stream),
                    EntryTag.InvokeDynamic => new InvokeDynamicEntry(stream),
                    _ => throw new ArgumentOutOfRangeException(nameof(tag))
                };
                entries.Add(entry);
                if (entry is LongEntry || entry is DoubleEntry)
                {
                    entries.Add(new LongDoublePlaceholderEntry());
                    i++;
                }
            }

            foreach (var entry in entries)
                entry.ProcessFromConstantPool(this);
        }

        private class LongDoublePlaceholderEntry : Entry
        {
            public override EntryTag Tag => throw new Exception("You shouldn't access that entry");

            public override void ProcessFromConstantPool(ConstantPool constantPool) { }

            public override void Write(Stream stream) { }

            public override void PutToConstantPool(ConstantPool constantPool) { }
        }

        public void Write(Stream stream)
        {
            if (entries.Count > ushort.MaxValue)
                throw new ArgumentException($"Too many entries: {entries.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort)(entries.Count + 1));
            foreach (var entry in entries)
            {
                stream.WriteByte((byte)entry.Tag);
                entry.Write(stream);
            }
        }
    }
}