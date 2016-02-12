namespace System
{
    static class IdentifierHash
    {
        const int Radix = ('z' - 'a' + 1) + ('9' - '0' + 1) + 1 + /*'_'*/ + 1 /* empty */; // 38
        const int Step = 12;
        const int StackAllowCount = 4;

        public static unsafe uint Hash(string identifier, uint seed)
        {
            ulong* raw = stackalloc ulong[StackAllowCount];

            if (identifier.Length > StackAllowCount * Step)
            {
                fixed (ulong* p = new ulong[identifier.Length / Step + 1])
                {
                    raw = p;
                }
            }

            uint k = 0;
            int pos = 0;
            int blockEnds = identifier.Length / Step * Step;
            
            while (pos < blockEnds)
            {
                int end = pos + Step;
                raw[k++] = ToLong(identifier, pos, end);
                pos = end;
            }

            if (pos < identifier.Length)
            {
                raw[k++] = ToLong(identifier, pos, identifier.Length);
            }

            return Murmur3.Hash32((byte*)raw, k * 8, seed);
        }

        private static ulong ToLong(string identifier, int start, int end)
        {
            ulong result = 0;
            ulong acc = 1;

            unchecked
            {
                for (int i = start; i < end; i++)
                {
                    int ch = identifier[i];
                    if (ch >= 'a' && ch <= 'z') ch -= 'a';
                    else if (ch >= 'A' && ch <= 'Z') ch -= 'A';
                    else if (ch >= '0' && ch <= '9') ch += 26 - '0';
                    else if (ch == '_') ch = 36;
                    else throw new ArgumentException($"IdentifierId only support 'A-Z', 'a-z', '0-9' or '-'. Got '{identifier[i]}'");

                    result += acc * ((ulong)ch + 1);
                    acc *= Radix;
                }
            }

            return result;
        }
    }
}