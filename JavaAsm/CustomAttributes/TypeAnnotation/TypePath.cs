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
            var pathSize = stream.ReadByteFully();
            Path.Capacity = pathSize;
            for (var i = 0; i < pathSize; i++)
            {
                Path.Add(new PathPart
                {
                    TypePathKind = (TypePathKind) stream.ReadByteFully(),
                    TypeArgumentIndex = stream.ReadByteFully()
                });
            }
        }

        internal void Write(Stream stream, ClassWriterState writerState)
        {
            if (Path.Count > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"Path is too big: {Path.Count} > {byte.MaxValue}");
            stream.WriteByte((byte) Path.Count);
            foreach (var part in Path)
            {
                stream.WriteByte((byte) part.TypePathKind);
                stream.WriteByte(part.TypeArgumentIndex);
            }
        }
    }
}