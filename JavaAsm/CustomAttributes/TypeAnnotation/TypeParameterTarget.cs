using System.IO;
using JavaAsm.Helpers;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class TypeParameterTarget : TypeAnnotationTarget
    {
        public byte TypeParameterIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.TypeParameter;

        internal override void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte(this.TypeParameterIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            this.TypeParameterIndex = stream.ReadByteFully();
        }
    }
}