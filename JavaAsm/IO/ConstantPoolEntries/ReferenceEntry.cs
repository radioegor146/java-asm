using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal abstract class ReferenceEntry : Entry
    {
        public ClassEntry Class { get; private set; }
        private ushort classIndex;

        public NameAndTypeEntry NameAndType { get; private set; }
        private ushort nameAndTypeIndex;

        protected ReferenceEntry(ClassEntry @class, NameAndTypeEntry nameAndType)
        {
            this.Class = @class ?? throw new ArgumentNullException(nameof(@class));
            this.NameAndType = nameAndType ?? throw new ArgumentNullException(nameof(nameAndType));
        }

        protected ReferenceEntry(Stream stream)
        {
            this.classIndex = Binary.BigEndian.ReadUInt16(stream);
            this.nameAndTypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            this.Class = constantPool.GetEntry<ClassEntry>(this.classIndex);
            this.NameAndType = constantPool.GetEntry<NameAndTypeEntry>(this.nameAndTypeIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, this.classIndex);
            Binary.BigEndian.Write(stream, this.nameAndTypeIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            this.classIndex = constantPool.Find(this.Class);
            this.nameAndTypeIndex = constantPool.Find(this.NameAndType);
        }

        private bool Equals(ReferenceEntry other)
        {
            return this.Class.Equals(other.Class) && this.NameAndType.Equals(other.NameAndType);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ReferenceEntry)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Class.GetHashCode() * 397) ^ this.NameAndType.GetHashCode();
            }
        }
    }
}