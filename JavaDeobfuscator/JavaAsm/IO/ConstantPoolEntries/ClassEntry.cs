using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class ClassEntry : Entry
    {
        public Utf8Entry Name { get; private set; }
        private ushort nameIndex;

        public ClassEntry(Utf8Entry name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public ClassEntry(Stream stream)
        {
            nameIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.Class;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            Name = constantPool.GetEntry<Utf8Entry>(nameIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, nameIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            nameIndex = constantPool.Find(Name);
        }

        private bool Equals(ClassEntry other)
        {
            return Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ClassEntry)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}