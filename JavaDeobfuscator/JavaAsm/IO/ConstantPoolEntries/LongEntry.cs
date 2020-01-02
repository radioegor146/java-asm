using System.IO;
using BinaryEncoding;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class LongEntry : Entry
    {
        public long Value { get; }

        public LongEntry(long value)
        {
            Value = value;
        }

        public LongEntry(Stream stream)
        {
            Value = Binary.BigEndian.ReadInt64(stream);
        }

        public override EntryTag Tag => EntryTag.Long;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, Value);
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(LongEntry other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((LongEntry)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}