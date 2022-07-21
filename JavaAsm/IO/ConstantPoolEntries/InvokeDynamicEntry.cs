using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal class InvokeDynamicEntry : Entry
    {
        public ushort BootstrapMethodAttributeIndex { get; }

        public NameAndTypeEntry NameAndType { get; private set; }
        private ushort nameAndTypeIndex;

        public InvokeDynamicEntry(ushort bootstrapMethodAttributeIndex, NameAndTypeEntry nameAndType)
        {
            this.BootstrapMethodAttributeIndex = bootstrapMethodAttributeIndex;
            this.NameAndType = nameAndType ?? throw new ArgumentNullException(nameof(nameAndType));
        }

        public InvokeDynamicEntry(Stream stream)
        {
            this.BootstrapMethodAttributeIndex = Binary.BigEndian.ReadUInt16(stream);
            this.nameAndTypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.InvokeDynamic;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            this.NameAndType = constantPool.GetEntry<NameAndTypeEntry>(this.nameAndTypeIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, this.BootstrapMethodAttributeIndex);
            Binary.BigEndian.Write(stream, this.nameAndTypeIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            this.nameAndTypeIndex = constantPool.Find(this.NameAndType);
        }

        private bool Equals(InvokeDynamicEntry other)
        {
            return this.BootstrapMethodAttributeIndex == other.BootstrapMethodAttributeIndex && Equals(this.NameAndType, other.NameAndType);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((InvokeDynamicEntry)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.BootstrapMethodAttributeIndex.GetHashCode() * 397) ^ (this.NameAndType != null ? this.NameAndType.GetHashCode() : 0);
            }
        }
    }
}