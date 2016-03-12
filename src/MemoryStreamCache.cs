using System.Threading;
using System.Text;

namespace System.IO
{
    internal static class MemoryStreamCache
    {
        private const int MAX_BUILDER_SIZE = 1024;
        private const int DEFAULT_CAPACITY = 256;

        [ThreadStatic]
        private static MemoryStream t_cachedInstance;

        public static MemoryStream Acquire(int capacity = DEFAULT_CAPACITY)
        {
            if (capacity <= MAX_BUILDER_SIZE)
            {
                MemoryStream ms = MemoryStreamCache.t_cachedInstance;
                if (ms != null)
                {
                    // Avoid MemoryStream block fragmentation by getting a new MemoryStream
                    // when the requested size is larger than the current capacity
                    if (capacity <= ms.Capacity)
                    {
                        MemoryStreamCache.t_cachedInstance = null;
                        ms.Seek(0, SeekOrigin.Begin);
                        return ms;
                    }
                }
            }
            return new MemoryStream(capacity);
        }

        public static void Release(MemoryStream ms)
        {
            if (ms.Capacity <= MAX_BUILDER_SIZE)
            {
                MemoryStreamCache.t_cachedInstance = ms;
            }
        }

        public static byte[] GetBytesAndRelease(MemoryStream ms)
        {
            byte[] result = ms.ToArray();
            Release(ms);
            return result;
        }
    }
}

