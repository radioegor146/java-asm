using System.IO;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal class MethodReferenceEntry : ReferenceEntry
    {
        public MethodReferenceEntry(ClassEntry @class, NameAndTypeEntry nameAndType) : base(@class, nameAndType) { }

        public override EntryTag Tag => EntryTag.MethodRef;

        public MethodReferenceEntry(Stream stream) : base(stream) { }
    }
}