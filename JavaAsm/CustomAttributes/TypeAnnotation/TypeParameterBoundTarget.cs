using System.IO;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class TypeParameterBoundTarget : TypeAnnotationTarget
    {
        public byte TypeParameterIndex { get; set; }

        public byte BoundIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.TypeParameterBound;

        internal override void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte(TypeParameterIndex);
            stream.WriteByte(BoundIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            TypeParameterIndex = (byte) stream.ReadByte();
            BoundIndex = (byte) stream.ReadByte();
        }
    }
}