using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class StringEntry : Entry
    {
        public Utf8Entry Value { get; private set; }
        private ushort nameIndex;

        public StringEntry(Utf8Entry @string)
        {
            Value = @string ?? throw new ArgumentNullException(nameof(@string));
        }

        public StringEntry(Stream stream)
        {
            nameIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.String;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            Value = constantPool.GetEntry<Utf8Entry>(nameIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, nameIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            nameIndex = constantPool.Find(Value);
        }

        private bool Equals(StringEntry other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((StringEntry)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}