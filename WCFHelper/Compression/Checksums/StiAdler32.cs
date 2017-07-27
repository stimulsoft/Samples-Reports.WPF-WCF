using System;

namespace WCFHelper.Compression
{
    internal sealed class StiAdler32
    {
        #region Fields
        internal uint checksum;
        #endregion

        #region Consts
        private const uint BASE = 65521;
        #endregion

        #region Properties
        internal long Value
        {
            get
            {
                return checksum;
            }
        }
        #endregion

        #region Methods
        public StiAdler32()
        {
            checksum = 1;
        }

        public void Update(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "cannot be negative");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "cannot be negative");

            if (offset >= buffer.Length)
                throw new ArgumentOutOfRangeException("offset", "not a valid index into buffer");

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count", "exceeds buffer size");

            var s1 = checksum & 0xFFFF;
            var s2 = checksum >> 16;

            while (count > 0)
            {
                var n = 3800;
                if (n > count)
                    n = count;

                count -= n;
                while (--n >= 0)
                {
                    s1 = s1 + (uint)(buffer[offset++] & 0xff);
                    s2 = s2 + s1;
                }

                s1 %= BASE;
                s2 %= BASE;
            }

            checksum = (s2 << 16) | s1;
        }
        #endregion
    }
}