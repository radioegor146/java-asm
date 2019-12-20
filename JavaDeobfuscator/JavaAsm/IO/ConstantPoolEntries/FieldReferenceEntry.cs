using System.IO;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class FieldReferenceEntry : ReferenceEntry
    {
        public FieldReferenceEntry(ClassEntry @class, NameAndTypeEntry nameAndType) : base(@class, nameAndType) { }

        public override EntryTag Tag => EntryTag.FieldRef;

        public FieldReferenceEntry(Stream stream) : base(stream) { }
    }
}