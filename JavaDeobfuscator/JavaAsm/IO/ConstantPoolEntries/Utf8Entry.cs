using System;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.Helpers;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class Utf8Entry : Entry
    {
        public string String { get; }

        public Utf8Entry(string @string)
        {
            String = @string ?? throw new ArgumentNullException(nameof(@string));
        }

        public Utf8Entry(Stream stream)
        {
            var data = new byte[Binary.BigEndian.ReadUInt16(stream)];
            stream.Read(data);
            String = ModifiedUtf8Helper.Decode(data);
        }

        public override EntryTag Tag => EntryTag.Utf8;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, ModifiedUtf8Helper.GetBytesCount(String));
            stream.Write(ModifiedUtf8Helper.Encode(String));
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(Utf8Entry other)
        {
            return String == other.String;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Utf8Entry)obj);
        }

        public override int GetHashCode()
        {
            return String.GetHashCode();
        }
    }
}