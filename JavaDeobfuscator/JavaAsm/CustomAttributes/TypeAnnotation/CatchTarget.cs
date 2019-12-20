using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class CatchTarget : ITypeAnnotationTarget
    {
        public ushort ExceptionTableIndex { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.Catch;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, ExceptionTableIndex);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            ExceptionTableIndex = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}