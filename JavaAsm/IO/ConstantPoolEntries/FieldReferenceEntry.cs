using System.IO;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal class FieldReferenceEntry : ReferenceEntry
    {
        public FieldReferenceEntry(ClassEntry @class, NameAndTypeEntry nameAndType) : base(@class, nameAndType) { }

        public override EntryTag Tag => EntryTag.FieldReference;

        public FieldReferenceEntry(Stream stream) : base(stream) { }
    }
}