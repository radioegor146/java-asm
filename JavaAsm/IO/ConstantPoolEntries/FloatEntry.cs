using System;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries {
    internal class FloatEntry : Entry {
        public float Value { get; }

        public FloatEntry(float value) {
            this.Value = value;
        }

        public FloatEntry(Stream stream) {
            unsafe {
                int value = Binary.BigEndian.ReadInt32(stream);
                this.Value = *(float*) &value;
            }
        }

        public override EntryTag Tag => EntryTag.Float;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream) {
            unsafe {
                float value = this.Value;
                Binary.BigEndian.Write(stream, *(int*) &value);
            }
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(FloatEntry other) {
            return this.Value.Equals(other.Value);
        }

        public override bool Equals(object obj) {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((FloatEntry) obj);
        }

        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }
    }
}