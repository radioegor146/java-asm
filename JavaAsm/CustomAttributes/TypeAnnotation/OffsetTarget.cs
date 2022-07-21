using System.IO;
using BinaryEncoding;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class OffsetTarget : TypeAnnotationTarget
    {
        public ushort Offset { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.Offset;

        internal override void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, this.Offset);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            this.Offset = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}