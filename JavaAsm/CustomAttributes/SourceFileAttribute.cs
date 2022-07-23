using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes {
    public class SourceFileAttribute : CustomAttribute {
        public string Value { get; set; }

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            Binary.BigEndian.Write(attributeDataStream, writerState.ConstantPool.Find(new Utf8Entry(this.Value)));

            return attributeDataStream.ToArray();
        }
    }

    internal class SourceFileAttributeFactory : ICustomAttributeFactory<SourceFileAttribute> {
        public SourceFileAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            return new SourceFileAttribute {
                Value = readerState.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).String
            };
        }
    }
}