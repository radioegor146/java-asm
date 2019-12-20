using System.IO;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class TypeParameterBoundTarget : ITypeAnnotationTarget
    {
        public byte TypeParameterIndex { get; set; }

        public byte BoundIndex { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.TypeParameterBound;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte(TypeParameterIndex);
            stream.WriteByte(BoundIndex);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            TypeParameterIndex = (byte) stream.ReadByte();
            BoundIndex = (byte) stream.ReadByte();
        }
    }
}