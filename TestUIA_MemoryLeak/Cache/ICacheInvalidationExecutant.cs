using System;

namespace TestUIA.Cache
{
    public interface ICacheInvalidationExecutant : IDisposable
    {
        event EventHandler<InvalidateEventArgs> Invalidate;

        bool Init(int processId, IntPtr windowHandle);
    }

    public abstract class InvalidateEventArgs : EventArgs
    {
        protected InvalidateEventArgs(DateTime timestampUtc = default(DateTime))
        {
            TimestampUtc = timestampUtc != default(DateTime) ? timestampUtc : DateTime.UtcNow;
        }

        public DateTime TimestampUtc { get; private set; }
    }

    public abstract class PartialInvalidateEventArgs : InvalidateEventArgs
    {
        protected PartialInvalidateEventArgs(DateTime timestampUtc = default(DateTime))
            : base(timestampUtc)
        {
        }
    }
}