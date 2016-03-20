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

            for (var i = 0; i < 2; i++)
            {
                var encrypted = Rsa.Encrypt(publicKey, "hello world");

                Console.WriteLine();
                Console.WriteLine("encrypted:");
                Console.WriteLine(encrypted);

                Assert.Equal("hello world", Rsa.Decrypt(privateKey, encrypted));
            }

            for (var i = 0; i < 2; i++)
            {
                var longText = "Nam illud iudico no. Est et oratio labore feugiat. Ius paulo dolores gloriatur ad. Eam senserit intellegebat cu, in error nusquam ponderum duo, paulo simul omnes cu his. Posse admodum ea duo.";
                var encrypted = RsaAes.Encrypt(publicKey, longText);

                Console.WriteLine();
                Console.WriteLine("encrypted:");
                Console.WriteLine(encrypted);

                Assert.Equal(longText, RsaAes.Decrypt(privateKey, encrypted));
            }
        }
    }
}
