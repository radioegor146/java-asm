using System;
using System.IO;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class DeprecatedAttribute : CustomAttribute
    {
        public override byte[] Save(ClassWriterState writerState, AttributeScope scope) => new byte[0];
    }

    internal class DeprecatedAttributeFactory : ICustomAttributeFactory<DeprecatedAttribute>
    {
        public DeprecatedAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) => new DeprecatedAttribute();
    }
}