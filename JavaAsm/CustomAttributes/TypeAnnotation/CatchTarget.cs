using System.IO;
using BinaryEncoding;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class CatchTarget : TypeAnnotationTarget
    {
        public ushort ExceptionTableIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.Catch;

        internal override void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, this.ExceptionTableIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            this.ExceptionTableIndex = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}