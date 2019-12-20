using System.IO;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class EmptyTarget : ITypeAnnotationTarget
    {
        public TargetTypeKind TargetTypeKind => TargetTypeKind.Empty;

        public void Write(Stream stream, ClassWriterState writerState) { }

        public void Read(Stream stream, ClassReaderState readerState) { }
    }
}