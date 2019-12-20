using System;
using System.Collections.Generic;
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
        public SyntheticAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            if (attributeNode.Data.Length != 0)
                throw new ArgumentOutOfRangeException($"Attribute length is incorrect for Synthetic: {attributeNode.Data.Length} != {0}");
            return new SyntheticAttribute();
        }
    }
}
