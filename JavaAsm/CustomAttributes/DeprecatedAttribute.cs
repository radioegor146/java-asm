using System.IO;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes
{
    public class DeprecatedAttribute : CustomAttribute
    {
        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) => new byte[0];
    }

    internal class DeprecatedAttributeFactory : ICustomAttributeFactory<DeprecatedAttribute>
    {
        public DeprecatedAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) => new DeprecatedAttribute();
    }
}