using System;
using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;

namespace JavaAsm.IO.ConstantPoolEntries {
    internal class Utf8Entry : Entry {
        public string String { get; }

        public Utf8Entry(string @string) {
            this.String = @string ?? throw new ArgumentNullException(nameof(@string));
        }

        public Utf8Entry(Stream stream) {
            byte[] data = new byte[Binary.BigEndian.ReadUInt16(stream)];
            stream.Read(data, 0, data.Length);
            this.String = ModifiedUtf8Helper.Decode(data);
        }

        public override EntryTag Tag => EntryTag.Utf8;

        public override void ProcessFromConstantPool(ConstantPool constantPool) { }

        public override void Write(Stream stream) {
            Binary.BigEndian.Write(stream, ModifiedUtf8Helper.GetBytesCount(this.String));
            stream.Write(ModifiedUtf8Helper.Encode(this.String));
        }

        public override void PutToConstantPool(ConstantPool constantPool) { }

        private bool Equals(Utf8Entry other) {
            return this.String == other.String;
        }

        public override bool Equals(object obj) {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((Utf8Entry) obj);
        }

        public override int GetHashCode() {
            return this.String.GetHashCode();
        }
    }
}