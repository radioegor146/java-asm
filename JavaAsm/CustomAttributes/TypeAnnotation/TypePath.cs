using System;
using System.Collections.Generic;
using System.IO;
using JavaAsm.Helpers;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class TypePath
    {
        public enum TypePathKind
        {
            DeeperInArray,
            DeeperInNested,
            Wildcard,
            Type
        }

        public class PathPart
        {
            public TypePathKind TypePathKind { get; set; }

            public byte TypeArgumentIndex { get; set; }
        }

        public List<PathPart> Path { get; set; } = new List<PathPart>();

        internal void Read(Stream stream, ClassReaderState classReaderState)
        {
            byte pathSize = stream.ReadByteFully();
            this.Path.Capacity = pathSize;
            for (int i = 0; i < pathSize; i++)
            {
                this.Path.Add(new PathPart
                {
                    TypePathKind = (TypePathKind) stream.ReadByteFully(),
                    TypeArgumentIndex = stream.ReadByteFully()
                });
            }
        }

        internal void Write(Stream stream, ClassWriterState writerState)
        {
            if (this.Path.Count > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Path.Count), $"Path is too big: {this.Path.Count} > {byte.MaxValue}");
            stream.WriteByte((byte) this.Path.Count);
            foreach (PathPart part in this.Path)
            {
                stream.WriteByte((byte) part.TypePathKind);
                stream.WriteByte(part.TypeArgumentIndex);
            }
        }
    }
}