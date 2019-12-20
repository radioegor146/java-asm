using System.IO;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class TypeParameterTarget : ITypeAnnotationTarget
    {
        public byte TypeParameterIndex { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.TypeParameter;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte(TypeParameterIndex);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            TypeParameterIndex = (byte) stream.ReadByte();
        }
    }
}