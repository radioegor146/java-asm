using System.IO;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class EmptyTarget : TypeAnnotationTarget
    {
        public override TargetTypeKind TargetTypeKind => TargetTypeKind.Empty;

        internal override void Write(Stream stream, ClassWriterState writerState) { }

        internal override void Read(Stream stream, ClassReaderState readerState) { }
    }
}