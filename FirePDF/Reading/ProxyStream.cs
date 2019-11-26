using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Reading
{
    class ProxyStream : Stream
    {
        private Stream stream;
        private long position;
        private int length;

        public ProxyStream(Stream stream, long position, int length)
        {
            this.stream = stream;
            this.position = position;
            this.length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position { get => stream.Position - position; set => stream.Position = position + value; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, Math.Min(count, length - (int)Position));

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(position + offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
