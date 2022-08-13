using System.IO;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes {
    public class SyntheticAttribute : CustomAttribute {
        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) => new byte[0];
    }

    internal class SyntheticAttributeFactory : ICustomAttributeFactory<SyntheticAttribute> {
        public SyntheticAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) => new SyntheticAttribute();
    }
}