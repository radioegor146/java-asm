using System.IO;
using JavaAsm.Helpers;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes
{
    public class SourceDebugExtensionAttribute : CustomAttribute
    {
        public string Value { get; set; }

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            return ModifiedUtf8Helper.Encode(this.Value);
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
