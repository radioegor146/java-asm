using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class SyntheticAttribute : CustomAttribute
    {
        public override byte[] Save(ClassWriterState writerState, AttributeScope scope) => new byte[0];
    }

    internal class SyntheticAttributeFactory : ICustomAttributeFactory<SyntheticAttribute>
    {
        public SyntheticAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) => new SyntheticAttribute();
    }
}
