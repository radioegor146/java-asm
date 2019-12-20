using System.IO;
using BinaryEncoding;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class IntegerEntry : Entry
    {
        public int Value { get; }

        public IntegerEntry(int value)
        {
            Value = value;
        }

        public IntegerEntry(Stream stream)
        {
            Value = Binary.BigEndian.ReadInt32(stream);
        }

        public override EntryTag Tag => EntryTag.Integer;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, Value);
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(IntegerEntry other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IntegerEntry)obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}