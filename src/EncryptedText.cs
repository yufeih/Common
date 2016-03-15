namespace System.Security.Cryptography
{
    using System;
    using System.IO;

    public static class EncryptedText
    {
        public static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using (var rij = new RijndaelManaged { Key = key, IV = iv })
            {
                var encryptor = rij.CreateEncryptor(rij.Key, rij.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, byte[] key, byte[] iv)
        {
            using (var rij = new RijndaelManaged { Key = key, IV = iv })
            {
                var decryptor = rij.CreateDecryptor(rij.Key, rij.IV);

                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
