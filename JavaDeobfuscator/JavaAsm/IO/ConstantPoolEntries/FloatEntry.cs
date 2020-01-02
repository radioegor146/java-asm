using System;
using System.IO;
using BinaryEncoding;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class FloatEntry : Entry
    {
        public float Value { get; }

        public FloatEntry(float value)
        {
            Value = value;
        }

        public FloatEntry(Stream stream)
        {
            Value = BitConverter.Int32BitsToSingle(Binary.BigEndian.ReadInt32(stream));
        }

        public override EntryTag Tag => EntryTag.Float;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, BitConverter.SingleToInt32Bits(Value));
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(FloatEntry other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((FloatEntry)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}