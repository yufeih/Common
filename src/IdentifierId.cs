namespace System
{
    using System.Text;

    /// <summary>
    /// Store case insensitive identifiers (a-z,0-9, less than or equal to 25 charactors) 
    /// using 2 ulongs (128bit).
    /// </summary>
    struct IdentifierId : IComparable<IdentifierId>, IEquatable<IdentifierId>
    {
        private const int Radix = 'z' - 'a' + 1 + '9' - '0' + 1 + 1; // 37
        public const int MaxLength = 24;

        public readonly ulong A;
        public readonly ulong B;

        public static readonly IdentifierId Empty = default(IdentifierId);

        public static implicit operator IdentifierId(string id) => new IdentifierId(id);
        public static implicit operator string(IdentifierId id) => id.ToString();

        public IdentifierId(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                A = B = 0;
                return;
            }
            if (identifier.Length > MaxLength)
            {
                throw new ArgumentOutOfRangeException($"identifier longer than the maximum allowed length '{MaxLength}'");
            }

            A = ToLong(identifier, 0, Math.Min(MaxLength / 2, identifier.Length));

            if (identifier.Length > MaxLength / 2)
            {
                B = ToLong(identifier, MaxLength / 2, identifier.Length);
            }
            else
            {
                B = 0;
            }
        }

        public override string ToString()
        {
            if (A == 0) return "";

            var sb = new StringBuilder(MaxLength);

            ToString(A, sb);
            if (B > 0) ToString(B, sb);

            return sb.ToString();
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
                    else throw new ArgumentException($"IdentifierId only support A-Z,a-z,0-9. Got '{ch}'");

                    result += acc * ((ulong)ch + 1);
                    acc *= Radix;
                }
            }

            return result;
        }

        private static void ToString(ulong value, StringBuilder sb)
        {
            unchecked
            {
                while (true)
                {
                    int rem = (int)(value % Radix);

                    value /= Radix;

                    if (rem < 26) sb.Append((char)(rem - 1 + 'a'));
                    else sb.Append((char)(rem - 27 + '0'));

                    if (value == 0) break;
                }
            }
        }

        public override bool Equals(object obj) => obj is IdentifierId && Equals((IdentifierId)obj);
        public bool Equals(IdentifierId other) => B == other.B && A == other.A;

        public static bool operator ==(IdentifierId a, IdentifierId b) => a.A == b.A && a.B == b.B;
        public static bool operator !=(IdentifierId a, IdentifierId b) => a.A != b.A || a.B != b.B;

        public int CompareTo(IdentifierId other)
        {
            var result = A.CompareTo(other.A);
            if (result != 0) return result;
            return B.CompareTo(other.B);
        }

        public override int GetHashCode()
        {
            var b = B.GetHashCode();
            return (b << 5 + b) ^ A.GetHashCode();
        }
    }
}