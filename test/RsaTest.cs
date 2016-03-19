namespace System.Security.Cryptography
{
    using Xunit;

    public class RsaTest
    {
        [Fact]
        public void rsa_encrypt_decrypt()
        {
            string publicKey, privateKey;

            Rsa.CreateKeys(out publicKey, out privateKey);

            Console.WriteLine();
            Console.WriteLine("public key:");
            Console.WriteLine(publicKey);

            Console.WriteLine();
            Console.WriteLine("private key:");
            Console.WriteLine(privateKey);

            var encrypted = Rsa.Encrypt(publicKey, "hello world");

            Console.WriteLine();
            Console.WriteLine("encrypted:");
            Console.WriteLine(encrypted);

            Assert.Equal("hello world", Rsa.Decrypt(privateKey, encrypted));
        }
    }
}
