using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries {
    internal class NameAndTypeEntry : Entry {
        public Utf8Entry Name { get; private set; }
        private ushort nameIndex;

        public Utf8Entry Descriptor { get; private set; }
        private ushort descriptorIndex;

        public NameAndTypeEntry(Utf8Entry name, Utf8Entry descriptor) {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public NameAndTypeEntry(Stream stream) {
            this.nameIndex = Binary.BigEndian.ReadUInt16(stream);
            this.descriptorIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.NameAndType;

        public override void ProcessFromConstantPool(ConstantPool constantPool) {
            this.Name = constantPool.GetEntry<Utf8Entry>(this.nameIndex);
            this.Descriptor = constantPool.GetEntry<Utf8Entry>(this.descriptorIndex);
        }

        public override void Write(Stream stream) {
            Binary.BigEndian.Write(stream, this.nameIndex);
            Binary.BigEndian.Write(stream, this.descriptorIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool) {
            this.nameIndex = constantPool.Find(this.Name);
            this.descriptorIndex = constantPool.Find(this.Descriptor);
        }

        private bool Equals(NameAndTypeEntry other) {
            return this.Name.Equals(other.Name) && this.Descriptor.Equals(other.Descriptor);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((NameAndTypeEntry) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            unchecked {
                return (this.Name.GetHashCode() * 397) ^ this.Descriptor.GetHashCode();
            }
        }
    }
}