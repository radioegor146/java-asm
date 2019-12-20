﻿using System.IO;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes.TypeAnnotation
{
    internal class FormalParameterTarget : ITypeAnnotationTarget
    {
        public byte FormalParameterIndex { get; set; }

        public TargetTypeKind TargetTypeKind => TargetTypeKind.FormalParameter;

        public void Write(Stream stream, ClassWriterState writerState)
        {
            stream.WriteByte(FormalParameterIndex);
        }

        public void Read(Stream stream, ClassReaderState readerState)
        {
            FormalParameterIndex = (byte)stream.ReadByte();
        }
    }
}