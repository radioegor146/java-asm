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
            BootstrapMethodAttributeIndex = bootstrapMethodAttributeIndex;
            NameAndType = nameAndType ?? throw new ArgumentNullException(nameof(nameAndType));
        }

        public InvokeDynamicEntry(Stream stream)
        {
            BootstrapMethodAttributeIndex = Binary.BigEndian.ReadUInt16(stream);
            nameAndTypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.FieldRef;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            NameAndType = constantPool.GetEntry<NameAndTypeEntry>(nameAndTypeIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, BootstrapMethodAttributeIndex);
            Binary.BigEndian.Write(stream, nameAndTypeIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            nameAndTypeIndex = constantPool.Find(NameAndType);
        }

        private bool Equals(InvokeDynamicEntry other)
        {
            return BootstrapMethodAttributeIndex == other.BootstrapMethodAttributeIndex && Equals(NameAndType, other.NameAndType);
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
                return (BootstrapMethodAttributeIndex.GetHashCode() * 397) ^ (NameAndType != null ? NameAndType.GetHashCode() : 0);
            }
        }
    }
}