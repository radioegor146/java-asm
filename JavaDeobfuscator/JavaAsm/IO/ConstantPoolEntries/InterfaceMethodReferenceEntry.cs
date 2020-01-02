using System.IO;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public class InterfaceMethodReferenceEntry : MethodReferenceEntry
    {
        public InterfaceMethodReferenceEntry(ClassEntry @class, NameAndTypeEntry nameAndType) : base(@class, nameAndType) { }

        public override EntryTag Tag => EntryTag.InterfaceMethodRef;

        public InterfaceMethodReferenceEntry(Stream stream) : base(stream) { }
    }
}