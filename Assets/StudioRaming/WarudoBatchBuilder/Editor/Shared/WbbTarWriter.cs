using System;
using System.IO;
using System.Text;

namespace StudioRaming.WarudoBatchBuilder
{
    public class WbbTarWriter : IDisposable
    {
        public const int BlockSize = 512;
        private const int NameOffset = 0;
        private const int NameLength = 100;
        private const int ModeOffset = 100;
        private const int UidOffset = 108;
        private const int GidOffset = 116;
        private const int SizeOffset = 124;
        private const int MtimeOffset = 136;
        private const int ChecksumOffset = 148;
        private const int TypeFlagOffset = 156;
        private const int MagicOffset = 257;
        private const int VersionOffset = 263;
        private const int DevMajorOffset = 329;
        private const int DevMinorOffset = 337;
        private const int PrefixOffset = 345;
        private const int PrefixLength = 155;
        private const long MaxEntrySize = 8589934591L;
        private const int CopyBufferSize = 64 * 1024;
        private const byte TypeFile = (byte)'0';
        private const byte TypeDirectory = (byte)'5';
        private const string FileMode = "0000777";
        private const string DirMode = "0000777";
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);
        private readonly byte[] header = new byte[BlockSize];
        private readonly byte[] zeroBlock = new byte[BlockSize];
        private byte[] copyBuffer;
        private Stream output;
        private bool disposed;
        public WbbTarWriter(Stream output)
        {
            if (output == null)
                throw new ArgumentNullException("output");
            if (!output.CanWrite)
                throw new ArgumentException("Stream is not writable.", "output");
            this.output = output;
        }

        public void AddDirectory(string entryPath)
        {
            EnsureUsable();
            if (string.IsNullOrEmpty(entryPath))
                throw new ArgumentException("Entry path is empty.", "entryPath");
            string path = entryPath.Replace('\\', '/');
            if (!path.EndsWith("/", StringComparison.Ordinal))
                path += "/";
            WriteHeader(path, 0L, TypeDirectory, DirMode);
        }

        public void AddFile(string entryPath, byte[] content)
        {
            EnsureUsable();
            byte[] data = content == null ? new byte[0] : content;
            WriteHeader(entryPath, data.Length, TypeFile, FileMode);
            if (data.Length > 0)
                output.Write(data, 0, data.Length);
            WritePadding(data.Length);
        }

        public void AddFile(string entryPath, string utf8Text)
        {
            AddFile(entryPath, Utf8.GetBytes(utf8Text == null ? string.Empty : utf8Text));
        }

        public bool AddFile(string entryPath, Stream source, long length)
        {
            EnsureUsable();
            if (source == null)
                throw new ArgumentNullException("source");
            if (length < 0L)
                throw new ArgumentOutOfRangeException("length");
            WriteHeader(entryPath, length, TypeFile, FileMode);
            if (copyBuffer == null)
                copyBuffer = new byte[CopyBufferSize];
            bool complete = true;
            long remaining = length;
            while (remaining > 0L)
            {
                int want = (int)Math.Min((long)copyBuffer.Length, remaining);
                int read;
                try
                {
                    read = source.Read(copyBuffer, 0, want);
                }
                catch
                {
                    read = 0;
                }

                if (read <= 0)
                {
                    complete = false;
                    Array.Clear(copyBuffer, 0, copyBuffer.Length);
                    while (remaining > 0L)
                    {
                        int fill = (int)Math.Min((long)copyBuffer.Length, remaining);
                        output.Write(copyBuffer, 0, fill);
                        remaining -= fill;
                    }

                    break;
                }

                output.Write(copyBuffer, 0, read);
                remaining -= read;
            }

            WritePadding(length);
            return complete;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            var stream = output;
            output = null;
            if (stream == null)
                return;
            stream.Write(zeroBlock, 0, BlockSize);
            stream.Write(zeroBlock, 0, BlockSize);
            stream.Flush();
        }

        private void EnsureUsable()
        {
            if (disposed || output == null)
                throw new ObjectDisposedException("WbbTarWriter");
        }

        private void WritePadding(long size)
        {
            int rem = (int)(size % BlockSize);
            if (rem != 0)
                output.Write(zeroBlock, 0, BlockSize - rem);
        }

        private void WriteHeader(string entryPath, long size, byte typeFlag, string mode)
        {
            if (string.IsNullOrEmpty(entryPath))
                throw new ArgumentException("Entry path is empty.", "entryPath");
            if (size > MaxEntrySize)
                throw new ArgumentOutOfRangeException("size", "Entry is too large for the USTAR size field.");
            Array.Clear(header, 0, BlockSize);
            string name, prefix;
            SplitName(entryPath, out name, out prefix);
            WriteText(name, NameOffset, NameLength);
            WriteText(mode, ModeOffset, 7);
            WriteText("0000000", UidOffset, 7);
            WriteText("0000000", GidOffset, 7);
            WriteText(ToOctal(size, 11), SizeOffset, 11);
            WriteText(ToOctal(0L, 11), MtimeOffset, 11);
            for (int i = 0; i < 8; i++)
                header[ChecksumOffset + i] = (byte)' ';
            header[TypeFlagOffset] = typeFlag;
            WriteText("ustar ", MagicOffset, 6);
            header[VersionOffset] = (byte)' ';
            header[VersionOffset + 1] = 0;
            WriteText("0000000", DevMajorOffset, 7);
            WriteText("0000000", DevMinorOffset, 7);
            WriteText(prefix, PrefixOffset, PrefixLength);
            int sum = 0;
            for (int i = 0; i < BlockSize; i++)
                sum += header[i];
            string checksum = ToOctal(sum, 6);
            WriteText(checksum, ChecksumOffset, 6);
            header[ChecksumOffset + 6] = 0;
            header[ChecksumOffset + 7] = (byte)' ';
            output.Write(header, 0, BlockSize);
        }

        private void WriteText(string text, int offset, int length)
        {
            if (string.IsNullOrEmpty(text) || length <= 0)
                return;
            byte[] bytes = Utf8.GetBytes(text);
            int count = bytes.Length < length ? bytes.Length : length;
            Buffer.BlockCopy(bytes, 0, header, offset, count);
        }

        private static string ToOctal(long value, int digits)
        {
            if (value < 0L)
                value = 0L;
            var buffer = new char[digits];
            for (int i = digits - 1; i >= 0; i--)
            {
                buffer[i] = (char)('0' + (int)(value & 7L));
                value >>= 3;
            }

            return new string (buffer);
        }

        private static void SplitName(string entryPath, out string name, out string prefix)
        {
            string path = entryPath.Replace('\\', '/');
            prefix = string.Empty;
            name = path;
            if (Utf8.GetByteCount(path) <= NameLength)
                return;
            for (int i = path.Length - 1; i > 0; i--)
            {
                if (path[i] != '/')
                    continue;
                string head = path.Substring(0, i);
                string tail = path.Substring(i + 1);
                if (Utf8.GetByteCount(tail) <= NameLength && Utf8.GetByteCount(head) <= PrefixLength)
                {
                    prefix = head;
                    name = tail;
                    return;
                }
            }

            name = TruncateUtf8(path, NameLength);
            prefix = string.Empty;
        }

        private static string TruncateUtf8(string text, int maxBytes)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (Utf8.GetByteCount(text) <= maxBytes)
                return text;
            int length = text.Length;
            while (length > 0 && Utf8.GetByteCount(text.Substring(0, length)) > maxBytes)
                length--;
            return text.Substring(0, length);
        }
    }
}
