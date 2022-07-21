using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class TypeArgumentTarget : TypeAnnotationTarget
    {
        public ushort Offset { get; set; }

        public byte TypeArgumentIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.TypeArgument;

        internal override void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, this.Offset);
            stream.WriteByte(this.TypeArgumentIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            this.Offset = Binary.BigEndian.ReadUInt16(stream);
            this.TypeArgumentIndex = stream.ReadByteFully();
        }
    }
}