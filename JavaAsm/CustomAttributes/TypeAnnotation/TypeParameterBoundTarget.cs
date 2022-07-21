using System.IO;
using JavaAsm.Helpers;
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
            stream.WriteByte(this.TypeParameterIndex);
            stream.WriteByte(this.BoundIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            this.TypeParameterIndex = stream.ReadByteFully();
            this.BoundIndex = stream.ReadByteFully();
        }
    }
}