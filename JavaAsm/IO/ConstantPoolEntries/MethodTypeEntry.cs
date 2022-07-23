using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries {
    internal class MethodTypeEntry : Entry {
        public Utf8Entry Descriptor { get; private set; }
        private ushort descriptorIndex;

        public MethodTypeEntry(Utf8Entry descriptor) {
            this.Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public MethodTypeEntry(Stream stream) {
            this.descriptorIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.MethodType;

        public override void ProcessFromConstantPool(ConstantPool constantPool) {
            this.Descriptor = constantPool.GetEntry<Utf8Entry>(this.descriptorIndex);
        }

        public override void Write(Stream stream) {
            Binary.BigEndian.Write(stream, this.descriptorIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool) {
            this.descriptorIndex = constantPool.Find(this.Descriptor);
        }

        private bool Equals(MethodTypeEntry other) {
            return Equals(this.Descriptor, other.Descriptor);
        }

        public override bool Equals(object obj) {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((MethodTypeEntry) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            return this.Descriptor != null ? this.Descriptor.GetHashCode() : 0;
        }
    }
}