using System;

namespace WCFHelper.Compression
{
    internal class StiDeflaterPending : StiPendingBuffer
    {
        public StiDeflaterPending()
            : base(StiDeflaterConstants.PENDING_BUF_SIZE)
        {
        }
    }
}