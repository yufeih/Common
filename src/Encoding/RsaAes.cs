namespace System.Security.Cryptography
{
    using System;
    using System.IO;
    using System.Text;

    static class RsaAes
    {
        public static void CreateKeys(out string publicKey, out string privateKey)
        {
            Rsa.CreateKeys(out publicKey, out privateKey);
        }

        public static byte[] Encrypt(string publicKey, byte[] data)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(Rsa.Encrypt(publicKey, aes.IV), 0, 128);
                    ms.Write(Rsa.Encrypt(publicKey, aes.Key), 0, 128);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        public static string Encrypt(string publicKey, string data)
        {
            return Convert.ToBase64String(Encrypt(publicKey, Encoding.UTF8.GetBytes(data)));
        }

        public static byte[] Decrypt(string privateKey, byte[] data)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                var iv = new byte[128];
                var key = new byte[128];

                Buffer.BlockCopy(data, 0, iv, 0, 128);
                Buffer.BlockCopy(data, 128, key, 0, 128);

                using (var encryptor = aes.CreateDecryptor(Rsa.Decrypt(privateKey, key), Rsa.Decrypt(privateKey, iv)))
                using (var src = new MemoryStream(data, writable: false))
                using (var dst = new MemoryStream())
                {
                    src.Seek(256, SeekOrigin.Begin);
                    using (var cs = new CryptoStream(src, encryptor, CryptoStreamMode.Read))
                    {
                        cs.CopyTo(dst);
                    }
                    return dst.ToArray();
                }
            }
        }

        public static string Decrypt(string privateKey, string data)
        {
            return Encoding.UTF8.GetString(Decrypt(privateKey, Convert.FromBase64String(data)));
        }
    }
}
