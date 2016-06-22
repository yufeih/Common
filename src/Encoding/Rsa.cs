namespace System.Security.Cryptography
{
    using System.Text;

    static class Rsa
    {
        public static void CreateKeys(out string publicKey, out string privateKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                publicKey = WritePublicKey(rsa.ExportParameters(false));
                privateKey = WritePrivateKey(rsa.ExportParameters(true));
            }
        }

        public static byte[] Encrypt(string publicKey, byte[] data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(ReadPublicKey(publicKey));

                return rsa.Encrypt(data, false);
            }
        }

        public static string Encrypt(string publicKey, string data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(ReadPublicKey(publicKey));

                return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(data), false));
            }
        }

        public static byte[] Decrypt(string privateKey, byte[] data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(ReadPrivateKey(privateKey));

                return rsa.Decrypt(data, false);
            }
        }

        public static string Decrypt(string privateKey, string data)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(ReadPrivateKey(privateKey));

                return Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(data), false));
            }
        }

        private static string WritePublicKey(RSAParameters param)
        {
            var bytes = new byte[3 + 128];

            Buffer.BlockCopy(param.Exponent, 0, bytes, 0, 3);
            Buffer.BlockCopy(param.Modulus, 0, bytes, 3, 128);

            return Convert.ToBase64String(bytes);
        }

        private static RSAParameters ReadPublicKey(string key)
        {
            var bytes = Convert.FromBase64String(key);
            var param = new RSAParameters
            {
                Exponent = new byte[3],
                Modulus = new byte[128],
            };

            Buffer.BlockCopy(bytes, 0, param.Exponent, 0, 3);
            Buffer.BlockCopy(bytes, 3, param.Modulus, 0, 128);

            return param;
        }

        private static string WritePrivateKey(RSAParameters param)
        {
            var bytes = new byte[128 + 64 + 64 + 3 + 64 + 128 + 64 + 64];

            Buffer.BlockCopy(param.D, 0, bytes, 0, 128);
            Buffer.BlockCopy(param.DP, 0, bytes, 128, 64);
            Buffer.BlockCopy(param.DQ, 0, bytes, 128 + 64, 64);
            Buffer.BlockCopy(param.Exponent, 0, bytes, 128 + 64 + 64, 3);
            Buffer.BlockCopy(param.InverseQ, 0, bytes, 128 + 64 + 64 + 3, 64);
            Buffer.BlockCopy(param.Modulus, 0, bytes, 128 + 64 + 64 + 3 + 64, 128);
            Buffer.BlockCopy(param.P, 0, bytes, 128 + 64 + 64 + 3 + 64 + 128, 64);
            Buffer.BlockCopy(param.Q, 0, bytes, 128 + 64 + 64 + 3 + 64 + 128 + 64, 64);

            return Convert.ToBase64String(bytes);
        }

        private static RSAParameters ReadPrivateKey(string key)
        {
            var bytes = Convert.FromBase64String(key);
            var param = new RSAParameters
            {
                D = new byte[128],
                DP = new byte[64],
                DQ = new byte[64],
                Exponent = new byte[3],
                InverseQ = new byte[64],
                Modulus = new byte[128],
                P = new byte[64],
                Q = new byte[64],
            };

            Buffer.BlockCopy(bytes, 0, param.D, 0, 128);
            Buffer.BlockCopy(bytes, 128, param.DP, 0, 64);
            Buffer.BlockCopy(bytes, 128 + 64, param.DQ, 0, 64);
            Buffer.BlockCopy(bytes, 128 + 64 + 64, param.Exponent, 0, 3);
            Buffer.BlockCopy(bytes, 128 + 64 + 64 + 3, param.InverseQ, 0, 64);
            Buffer.BlockCopy(bytes, 128 + 64 + 64 + 3 + 64, param.Modulus, 0, 128);
            Buffer.BlockCopy(bytes, 128 + 64 + 64 + 3 + 64 + 128, param.P, 0, 64);
            Buffer.BlockCopy(bytes, 128 + 64 + 64 + 3 + 64 + +128 + 64, param.Q, 0, 64);

            return param;
        }
    }
}
