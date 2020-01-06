using System.IO;
using JavaDeobfuscator.JavaAsm.Helpers;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class SourceDebugExtensionAttribute : CustomAttribute
    {
        public string Value { get; set; }

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            return ModifiedUtf8Helper.Encode(Value);
        }
    }

    internal class SourceDebugExtensionFactory : ICustomAttributeFactory<SourceDebugExtensionAttribute>
    {
        public SourceDebugExtensionAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            return new SourceDebugExtensionAttribute
            {
                Value = ModifiedUtf8Helper.Decode(attributeDataStream.ReadBytes(attributeDataLength))
            };
        }
    }
}
