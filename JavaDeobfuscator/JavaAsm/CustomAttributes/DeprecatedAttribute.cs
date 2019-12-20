using System;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class DeprecatedAttribute : CustomAttribute
    {
        public override byte[] Save(ClassWriterState writerState, AttributeScope scope) => new byte[0];
    }

    internal class DeprecatedAttributeFactory : ICustomAttributeFactory<DeprecatedAttribute>
    {
        public DeprecatedAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            if (attributeNode.Data.Length != 0)
                throw new ArgumentOutOfRangeException($"Attribute length is incorrect for Deprecated: {attributeNode.Data.Length} != {0}");
            return new DeprecatedAttribute();
        }
    }
}