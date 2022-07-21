using System.IO;
using JavaAsm.Helpers;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class FormalParameterTarget : TypeAnnotationTarget
    {
        public byte FormalParameterIndex { get; set; }

        public override TargetTypeKind TargetTypeKind => TargetTypeKind.FormalParameter;

        internal override void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte(this.FormalParameterIndex);
        }

        internal override void Read(Stream stream, ClassReaderState readerState)
        {
            this.FormalParameterIndex = stream.ReadByteFully();
        }
    }
}