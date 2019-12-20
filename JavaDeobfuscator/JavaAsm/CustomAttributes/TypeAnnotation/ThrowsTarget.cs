using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class ThrowsTarget : ITypeAnnotationTarget
    {
        public ushort ThrowsTypeIndex { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.Throws;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, ThrowsTypeIndex);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            ThrowsTypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}