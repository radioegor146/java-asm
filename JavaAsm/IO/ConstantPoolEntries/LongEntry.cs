using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal class LongEntry : Entry
    {
        public long Value { get; }

        public LongEntry(long value)
        {
            this.Value = value;
        }

        public LongEntry(Stream stream)
        {
            this.Value = Binary.BigEndian.ReadInt64(stream);
        }

        public override EntryTag Tag => EntryTag.Long;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, this.Value);
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(LongEntry other)
        {
            return this.Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((LongEntry)obj);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
    }
}