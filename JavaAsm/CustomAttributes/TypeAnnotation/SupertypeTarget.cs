using System.IO;
using BinaryEncoding;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation {
    public class SupertypeTarget : TypeAnnotationTarget {
        public ushort SupertypeIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.Supertype;

        internal override void Write(Stream stream, ClassWriterState writerState) {
            Binary.BigEndian.Write(stream, this.SupertypeIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState) {
            this.SupertypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}