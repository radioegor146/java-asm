using System;
using System.IO;

namespace JavaAsm.Helpers
{
    public class ReadWriteCountStream : Stream
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
            this.baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = this.baseStream.Read(buffer, offset, count);
            this.ReadBytes += Math.Max(0, result);
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();

        public override void SetLength(long value) => throw new InvalidOperationException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.baseStream.Write(buffer, offset, count);
            this.WrittenBytes += count;
        }

        public override bool CanRead => this.baseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => this.baseStream.CanWrite;

        public override long Length => this.baseStream.Length;

        public override long Position
        {
            get => this.baseStream.Position;
            set => this.baseStream.Position = value;
        }
    }
}
