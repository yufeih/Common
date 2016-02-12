namespace System
{
    using System.Runtime.CompilerServices;

    // Ported from https://github.com/PeterScott/murmur3/blob/master/murmur3.c
    static class Murmur3
    {
        public static unsafe uint Hash32(byte[] bytes, uint seed)
        {
            fixed (byte* p = bytes)
            {
                return Hash32(p, (uint)bytes.Length, seed);
            }
        }

        public static unsafe uint Hash32(byte[] bytes, int start, int length, uint seed)
        {
            fixed (byte* p = bytes)
            {
                return Hash32(p + start, (uint)length, seed);
            }
        }

        public static unsafe uint Hash32(byte* data, uint length, uint seed)
        {
            uint nblocks = length / 4;
            int i;

            uint h1 = seed;
            uint k1;

            uint c1 = 0xcc9e2d51;
            uint c2 = 0x1b873593;

            unchecked
            {
                //----------
                // body

                for (i = 0; i < nblocks; i++)
                {
                    k1 = *((uint*)data + i);

                    k1 *= c1;
                    k1 = rotl32(k1, 15);
                    k1 *= c2;

                    h1 ^= k1;
                    h1 = rotl32(h1, 13);
                    h1 = h1 * 5 + 0xe6546b64;
                }

                //----------
                // tail

                byte* tail = (byte*)(data + nblocks * 4);

                k1 = 0;

                switch (length & 3)
                {
                    case 3: k1 ^= (uint)tail[2] << 16; break;
                    case 2: k1 ^= (uint)tail[1] << 8; break;
                    case 1:
                        k1 ^= tail[0];
                        k1 *= c1; k1 = rotl32(k1, 15); k1 *= c2; h1 ^= k1;
                        break;
                };

                //----------
                // finalization

                h1 ^= length;

                h1 = fmix32(h1);
            }
            return h1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint fmix32(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;

            return h;
        }
    }
}