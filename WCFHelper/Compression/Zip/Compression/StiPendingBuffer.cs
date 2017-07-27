using System;

namespace WCFHelper.Compression
{
    internal class StiPendingBuffer
    {
        #region Fields
        private readonly byte[] buffer;

        private int start;
        private int end;

        private uint bits;
        public int BitCount;
        #endregion

        #region Methods
        public void Reset()
        {
            start = end = BitCount = 0;
        }

        public void WriteShort(int value)
        {
            this.buffer[end++] = unchecked((byte)value);
            this.buffer[end++] = unchecked((byte)(value >> 8));
        }

        public void WriteBlock(byte[] block, int offset, int length)
        {
            System.Array.Copy(block, offset, this.buffer, end, length);
            end += length;
        }

        public void AlignToByte()
        {
            if (BitCount > 0)
            {
                this.buffer[end++] = unchecked((byte)bits);
                if (BitCount > 8)
                {
                    this.buffer[end++] = unchecked((byte)(bits >> 8));
                }
            }
            bits = 0;
            BitCount = 0;
        }

        public void WriteBits(int b, int count)
        {
            bits |= (uint)(b << BitCount);
            BitCount += count;
            if (BitCount < 16)
            {
                return;
            }
            this.buffer[end++] = unchecked((byte)bits);
            this.buffer[end++] = unchecked((byte)(bits >> 8));
            bits >>= 16;
            BitCount -= 16;
        }

        public void WriteShortMSB(int s)
        {
            this.buffer[end++] = unchecked((byte)(s >> 8));
            this.buffer[end++] = unchecked((byte)s);
        }

        public int Flush(byte[] output, int offset, int length)
        {
            if (BitCount >= 8)
            {
                this.buffer[end++] = unchecked((byte)bits);
                bits >>= 8;
                BitCount -= 8;
            }

            if (length > end - start)
            {
                length = end - start;
                System.Array.Copy(this.buffer, start, output, offset, length);
                start = 0;
                end = 0;
            }
            else
            {
                System.Array.Copy(this.buffer, start, output, offset, length);
                start += length;
            }
            return length;
        }
        #endregion

        #region Properties
        public bool IsFlushed
        {
            get
            {
                return end == 0;
            }
        }
        #endregion        

        public StiPendingBuffer(int bufferSize)
        {
            this.buffer = new byte[bufferSize];
        }
    }
}