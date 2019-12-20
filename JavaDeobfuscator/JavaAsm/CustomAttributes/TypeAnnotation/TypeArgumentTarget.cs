using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class TypeArgumentTarget : ITypeAnnotationTarget
    {
        public ushort Offset { get; set; }

        public byte TypeArgumentIndex { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.TypeArgument;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, Offset);
            stream.WriteByte(TypeArgumentIndex);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            Offset = Binary.BigEndian.ReadUInt16(stream);
            TypeArgumentIndex = (byte) stream.ReadByte();
        }
    }
}