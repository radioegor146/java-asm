using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class SupertypeTarget : ITypeAnnotationTarget
    {
        public ushort SupertypeIndex { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.Supertype;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            Binary.BigEndian.Write(stream, SupertypeIndex);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            SupertypeIndex = Binary.BigEndian.ReadUInt16(stream);
        }
    }
}