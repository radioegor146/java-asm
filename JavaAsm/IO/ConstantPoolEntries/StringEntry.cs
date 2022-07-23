using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries {
    internal class StringEntry : Entry {
        public Utf8Entry Value { get; private set; }
        private ushort nameIndex;

        public StringEntry(Utf8Entry @string) {
            this.Value = @string ?? throw new ArgumentNullException(nameof(@string));
        }

        public StringEntry(Stream stream) {
            this.nameIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.String;

        public override void ProcessFromConstantPool(ConstantPool constantPool) {
            this.Value = constantPool.GetEntry<Utf8Entry>(this.nameIndex);
        }

        public override void Write(Stream stream) {
            Binary.BigEndian.Write(stream, this.nameIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool) {
            this.nameIndex = constantPool.Find(this.Value);
        }

        private bool Equals(StringEntry other) {
            return this.Value.Equals(other.Value);
        }

        public override bool Equals(object obj) {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((StringEntry) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }
    }
}