using System.IO;

namespace JavaAsm.IO.ConstantPoolEntries {
    internal abstract class Entry {
        public abstract EntryTag Tag { get; }

        public abstract void ProcessFromConstantPool(ConstantPool constantPool);

        public abstract void Write(Stream stream);

        public abstract void PutToConstantPool(ConstantPool constantPool);
    }
}