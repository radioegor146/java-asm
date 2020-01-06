using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal class NameAndTypeEntry : Entry
    {
        public Utf8Entry Name { get; private set; }
        private ushort nameIndex;

        public Utf8Entry Descriptor { get; private set; }
        private ushort descriptorIndex;

        public NameAndTypeEntry(Utf8Entry name, Utf8Entry descriptor)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public NameAndTypeEntry(Stream stream)
        {
            nameIndex = Binary.BigEndian.ReadUInt16(stream);
            descriptorIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.NameAndType;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            Name = constantPool.GetEntry<Utf8Entry>(nameIndex);
            Descriptor = constantPool.GetEntry<Utf8Entry>(descriptorIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, nameIndex);
            Binary.BigEndian.Write(stream, descriptorIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            nameIndex = constantPool.Find(Name);
            descriptorIndex = constantPool.Find(Descriptor);
        }

        private bool Equals(NameAndTypeEntry other)
        {
            return Name.Equals(other.Name) && Descriptor.Equals(other.Descriptor);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((NameAndTypeEntry)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ Descriptor.GetHashCode();
            }
        }
    }
}