using System.IO;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class MethodReferenceEntry : ReferenceEntry
    {
        public MethodReferenceEntry(ClassEntry @class, NameAndTypeEntry nameAndType) : base(@class, nameAndType) { }

        public override EntryTag Tag => EntryTag.MethodRef;

        public MethodReferenceEntry(Stream stream) : base(stream) { }
    }
}