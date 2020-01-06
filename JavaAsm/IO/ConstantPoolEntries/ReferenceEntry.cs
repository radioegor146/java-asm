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
            Class = @class ?? throw new ArgumentNullException(nameof(@class));
            NameAndType = nameAndType ?? throw new ArgumentNullException(nameof(nameAndType));
        }

        protected ReferenceEntry(Stream stream)
        {
            classIndex = Binary.BigEndian.ReadUInt16(stream);
            nameAndTypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            Class = constantPool.GetEntry<ClassEntry>(classIndex);
            NameAndType = constantPool.GetEntry<NameAndTypeEntry>(nameAndTypeIndex);
        }

        public override void Write(Stream stream)
        {
            Binary.BigEndian.Write(stream, classIndex);
            Binary.BigEndian.Write(stream, nameAndTypeIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            classIndex = constantPool.Find(Class);
            nameAndTypeIndex = constantPool.Find(NameAndType);
        }

        private bool Equals(ReferenceEntry other)
        {
            return Class.Equals(other.Class) && NameAndType.Equals(other.NameAndType);
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
                return (Class.GetHashCode() * 397) ^ NameAndType.GetHashCode();
            }
        }
    }
}