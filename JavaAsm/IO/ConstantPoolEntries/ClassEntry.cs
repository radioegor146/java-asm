using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal class ClassEntry : Entry
    {
        public Utf8Entry Name { get; private set; }
        private ushort nameIndex;

        public ClassEntry(Utf8Entry name)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public ClassEntry(Stream stream)
        {
            this.nameIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.Class;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            this.Name = constantPool.GetEntry<Utf8Entry>(this.nameIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, this.nameIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            this.nameIndex = constantPool.Find(this.Name);
        }

        private bool Equals(ClassEntry other)
        {
            return this.Name.Equals(other.Name);
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
            return this.Name.GetHashCode();
        }
    }
}