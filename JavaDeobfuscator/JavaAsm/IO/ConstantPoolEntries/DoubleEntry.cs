using System.IO;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class DoubleEntry : Entry
    {
        public double Value { get; }

        public DoubleEntry(double value)
        {
            Value = value;
        }

        public DoubleEntry(Stream stream)
        {
            using var reader = new BinaryReader(stream);
            Value = reader.ReadDouble();
        }

        public override EntryTag Tag => EntryTag.Double;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream)
        {
            using var writer = new BinaryWriter(stream);
            writer.Write(Value);
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(DoubleEntry other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((DoubleEntry)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}