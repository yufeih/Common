namespace System
{
    using System.Threading;

    /// <summary>
    /// Represents the lamport timestamp of this process.
    /// </summary>
    /// <remarks>
    ///  http://en.wikipedia.org/wiki/Lamport_timestamps
    /// </remarks>
    class LamportTimestamp
    {
        private long _stamp;

        public static readonly LamportTimestamp Current = new LamportTimestamp();

        public LamportTimestamp()
        {
            _stamp = DateTime.UtcNow.Ticks;
        }

        public LamportTimestamp(DateTime initial)
        {
            _stamp = initial.Ticks;
        }

        public DateTime Next()
        {
            var now = DateTime.UtcNow;
            var nowTicks = now.Ticks;
            var last = _stamp;

            if (nowTicks <= last)
            {
                return new DateTime(Interlocked.Increment(ref _stamp), DateTimeKind.Utc);
            }

            if (Interlocked.CompareExchange(ref _stamp, nowTicks, last) == last)
            {
                return now;
            }

            return new DateTime(Interlocked.Increment(ref _stamp), DateTimeKind.Utc);
        }

        public void EnsureAheadOf(DateTime time)
        {
            var stamp = _stamp;
            if (stamp < time.Ticks)
            {
                Interlocked.CompareExchange(ref _stamp, time.Ticks, stamp);
            }
        }
    }
}