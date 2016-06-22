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

                byte* tail = data + nblocks * 4;

                k1 = 0;

                switch (length & 3)
                {
                    case 3: k1 ^= (uint)tail[2] << 16; goto case 2;
                    case 2: k1 ^= (uint)tail[1] << 8; goto case 1;
                    case 1: k1 ^= tail[0]; k1 *= c1; k1 = rotl32(k1, 15); k1 *= c2; h1 ^= k1; break;
                };

                //----------
                // finalization

                h1 ^= length;

                h1 = fmix32(h1);
            }
            return h1;
        }

        public static unsafe void Hash128(byte[] bytes, uint seed, out ulong a, out ulong b)
        {
            fixed (byte* p = bytes)
            {
                Hash128(p, (uint)bytes.Length, seed, out a, out b);
            }
        }

        public static unsafe void Hash128(byte[] bytes, int start, int length, uint seed, out ulong a, out ulong b)
        {
            fixed (byte* p = bytes)
            {
                Hash128(p + start, (uint)length, seed, out a, out b);
            }
        }

        public static unsafe void Hash128(byte* data, uint length, uint seed, out ulong a, out ulong b)
        {
            uint nblocks = length / 16;
            int i;

            uint h1 = seed;
            uint h2 = seed;
            uint h3 = seed;
            uint h4 = seed;

            uint k1, k2, k3, k4;

            const uint c1 = 0x239b961b;
            const uint c2 = 0xab0e9789;
            const uint c3 = 0x38b34ae5;
            const uint c4 = 0xa1e38b93;

            unchecked
            {
                //----------
                // body

                for (i = 0; i < nblocks; i++)
                {
                    k1 = *((uint*)data + i * 4 + 0);
                    k2 = *((uint*)data + i * 4 + 1);
                    k3 = *((uint*)data + i * 4 + 2);
                    k4 = *((uint*)data + i * 4 + 3);

                    k1 *= c1; k1 = rotl32(k1, 15); k1 *= c2; h1 ^= k1;

                    h1 = rotl32(h1, 19); h1 += h2; h1 = h1 * 5 + 0x561ccd1b;

                    k2 *= c2; k2 = rotl32(k2, 16); k2 *= c3; h2 ^= k2;

                    h2 = rotl32(h2, 17); h2 += h3; h2 = h2 * 5 + 0x0bcaa747;

                    k3 *= c3; k3 = rotl32(k3, 17); k3 *= c4; h3 ^= k3;

                    h3 = rotl32(h3, 15); h3 += h4; h3 = h3 * 5 + 0x96cd1c35;

                    k4 *= c4; k4 = rotl32(k4, 18); k4 *= c1; h4 ^= k4;

                    h4 = rotl32(h4, 13); h4 += h1; h4 = h4 * 5 + 0x32ac3b17;
                }

                //----------
                // tail

                k1 = 0;
                k2 = 0;
                k3 = 0;
                k4 = 0;

                byte* tail = data + nblocks * 16;

                switch (length & 15)
                {
                    case 15: k4 ^= (uint)tail[14] << 16; goto case 14;
                    case 14: k4 ^= (uint)tail[13] << 8; goto case 13;
                    case 13: k4 ^= (uint)tail[12] << 0; k4 *= c4; k4 = rotl32(k4, 18); k4 *= c1; h4 ^= k4; goto case 12;

                    case 12: k3 ^= (uint)tail[11] << 24; goto case 11;
                    case 11: k3 ^= (uint)tail[10] << 16; goto case 10;
                    case 10: k3 ^= (uint)tail[9] << 8; goto case 9;
                    case 9: k3 ^= (uint)tail[8] << 0; k3 *= c3; k3 = rotl32(k3, 17); k3 *= c4; h3 ^= k3; goto case 8;

                    case 8: k2 ^= (uint)tail[7] << 24; goto case 7;
                    case 7: k2 ^= (uint)tail[6] << 16; goto case 6;
                    case 6: k2 ^= (uint)tail[5] << 8; goto case 5;
                    case 5: k2 ^= (uint)tail[4] << 0; k2 *= c2; k2 = rotl32(k2, 16); k2 *= c3; h2 ^= k2; goto case 4;

                    case 4: k1 ^= (uint)tail[3] << 24; goto case 3;
                    case 3: k1 ^= (uint)tail[2] << 16; goto case 2;
                    case 2: k1 ^= (uint)tail[1] << 8; goto case 1;
                    case 1: k1 ^= (uint)tail[0] << 0; k1 *= c1; k1 = rotl32(k1, 15); k1 *= c2; h1 ^= k1; break;
                }

                //----------
                // finalization

                h1 ^= length; h2 ^= length; h3 ^= length; h4 ^= length;

                h1 += h2; h1 += h3; h1 += h4;
                h2 += h1; h3 += h1; h4 += h1;

                h1 = fmix32(h1);
                h2 = fmix32(h2);
                h3 = fmix32(h3);
                h4 = fmix32(h4);

                h1 += h2; h1 += h3; h1 += h4;
                h2 += h1; h3 += h1; h4 += h1;

                a = (ulong)h2 << 32 | h1;
                b = (ulong)h4 << 32 | h3;
            }
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