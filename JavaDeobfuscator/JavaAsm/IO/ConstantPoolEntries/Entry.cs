using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    public abstract class Entry
    {
        public abstract EntryTag Tag { get; }

        public abstract void ProcessFromConstantPool(ConstantPool constantPool);

        public abstract void Write(Stream stream);

        public abstract void PutToConstantPool(ConstantPool constantPool);
    }
}
