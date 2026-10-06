using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbGzipStream : Stream
    {
        private const byte MagicOne = 0x1f;
        private const byte MagicTwo = 0x8b;
        private const byte MethodDeflate = 8;
        private const byte FlagFileName = 0x08;
        private const byte OsUnknown = 255;
        private static readonly uint[] CrcTable = BuildCrcTable();
        private readonly Stream output;
        private readonly bool leaveOpen;
        private DeflateStream deflate;
        private uint crc = 0xFFFFFFFFu;
        private long totalIn;
        private bool finished;
        public WbbGzipStream(Stream output, string originalFileName, uint modifiedUnixTime, bool leaveOpen = false)
        {
            if (output == null)
                throw new ArgumentNullException("output");
            if (!output.CanWrite)
                throw new ArgumentException("Stream is not writable.", "output");
            this.output = output;
            this.leaveOpen = leaveOpen;
            WriteHeader(originalFileName, modifiedUnixTime);
            deflate = new DeflateStream(output, CompressionMode.Compress, true);
        }

        private void WriteHeader(string originalFileName, uint modifiedUnixTime)
        {
            byte[] nameBytes = null;
            byte flags = 0;
            if (!string.IsNullOrEmpty(originalFileName))
            {
                if (IsAscii(originalFileName))
                {
                    nameBytes = Encoding.ASCII.GetBytes(originalFileName);
                    flags = FlagFileName;
                }
            }

            var header = new byte[10];
            header[0] = MagicOne;
            header[1] = MagicTwo;
            header[2] = MethodDeflate;
            header[3] = flags;
            header[4] = (byte)(modifiedUnixTime & 0xFF);
            header[5] = (byte)((modifiedUnixTime >> 8) & 0xFF);
            header[6] = (byte)((modifiedUnixTime >> 16) & 0xFF);
            header[7] = (byte)((modifiedUnixTime >> 24) & 0xFF);
            header[8] = 0;
            header[9] = OsUnknown;
            output.Write(header, 0, header.Length);
            if (nameBytes != null)
            {
                output.Write(nameBytes, 0, nameBytes.Length);
                output.WriteByte(0);
            }
        }

        private static bool IsAscii(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] > 127 || text[i] < 32)
                    return false;
            }

            return true;
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return !finished;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (finished)
                throw new ObjectDisposedException("WbbGzipStream");
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (count <= 0)
                return;
            deflate.Write(buffer, offset, count);
            uint value = crc;
            for (int i = 0; i < count; i++)
                value = CrcTable[(value ^ buffer[offset + i]) & 0xFF] ^ (value >> 8);
            crc = value;
            totalIn += count;
        }

        public override void Flush()
        {
            if (!finished && deflate != null)
                deflate.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing || finished)
            {
                base.Dispose(disposing);
                return;
            }

            finished = true;
            try
            {
                if (deflate != null)
                {
                    deflate.Dispose();
                    deflate = null;
                }

                uint finalCrc = crc ^ 0xFFFFFFFFu;
                uint size = (uint)(totalIn & 0xFFFFFFFFL);
                var footer = new byte[8];
                footer[0] = (byte)(finalCrc & 0xFF);
                footer[1] = (byte)((finalCrc >> 8) & 0xFF);
                footer[2] = (byte)((finalCrc >> 16) & 0xFF);
                footer[3] = (byte)((finalCrc >> 24) & 0xFF);
                footer[4] = (byte)(size & 0xFF);
                footer[5] = (byte)((size >> 8) & 0xFF);
                footer[6] = (byte)((size >> 16) & 0xFF);
                footer[7] = (byte)((size >> 24) & 0xFF);
                output.Write(footer, 0, footer.Length);
                output.Flush();
                if (!leaveOpen)
                    output.Dispose();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private static uint[] BuildCrcTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint value = i;
                for (int bit = 0; bit < 8; bit++)
                    value = (value & 1) != 0 ? 0xEDB88320u ^ (value >> 1) : value >> 1;
                table[i] = value;
            }

            return table;
        }
    }
}
