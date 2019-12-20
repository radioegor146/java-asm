using System.IO;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal interface ITypeAnnotationTarget
    {
        TargetTypeKind TargetTypeKind { get; }

        void Write(Stream stream, ClassWriterState writerState);

        void Read(Stream stream, ClassReaderState readerState);
    }
}