using System.IO;

namespace JavaAsm.IO.ConstantPoolEntries {
    internal class InterfaceMethodReferenceEntry : MethodReferenceEntry {
        public InterfaceMethodReferenceEntry(ClassEntry @class, NameAndTypeEntry nameAndType) : base(@class, nameAndType) { }

        public override EntryTag Tag => EntryTag.InterfaceMethodReference;

        public InterfaceMethodReferenceEntry(Stream stream) : base(stream) { }
    }
}