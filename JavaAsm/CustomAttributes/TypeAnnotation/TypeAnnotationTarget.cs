using System.IO;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public abstract class TypeAnnotationTarget
    {
        public abstract TargetTypeKind TargetTypeKind { get; }

        internal abstract void Write(Stream stream, ClassWriterState writerState);

        internal abstract void Read(Stream stream, ClassReaderState readerState);
    }
}