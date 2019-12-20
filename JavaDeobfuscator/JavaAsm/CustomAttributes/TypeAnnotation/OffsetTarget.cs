using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class OffsetTarget : ITypeAnnotationTarget
    {
        public ushort Offset { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.Offset;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, Offset);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            Offset = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}