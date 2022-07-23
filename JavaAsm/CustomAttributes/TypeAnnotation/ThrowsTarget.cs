using System.IO;
using BinaryEncoding;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation {
    public class ThrowsTarget : TypeAnnotationTarget {
        public ushort ThrowsTypeIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.Throws;

        internal override void Write(Stream stream, ClassWriterState writerState) {
            Binary.BigEndian.Write(stream, this.ThrowsTypeIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState) {
            this.ThrowsTypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}