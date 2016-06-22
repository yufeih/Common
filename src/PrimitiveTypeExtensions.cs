namespace System
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    static class PrimitiveTypeExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null) return "";

            // http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(87 + b + (((b - 10) >> 31) & -39));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(87 + b + (((b - 10) >> 31) & -39));
            }
            return new string(c);
        }
    }
}
