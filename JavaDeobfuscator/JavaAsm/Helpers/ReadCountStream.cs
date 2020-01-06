using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Helpers
{
    internal class ReadWriteCountStream : Stream
    {
        private readonly Stream baseStream;

        public long ReadBytes { get; private set; }
        public long WrittenBytes { get; private set; }

        public ReadWriteCountStream(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var result = baseStream.Read(buffer, offset, count);
            ReadBytes += Math.Max(0, result);
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();

        public override void SetLength(long value) => throw new InvalidOperationException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
            WrittenBytes += count;
        }

        public override bool CanRead => baseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => baseStream.CanWrite;

        public override long Length => baseStream.Length;

        public override long Position
        {
            get => baseStream.Position;
            set => baseStream.Position = value;
        }
    }
}
