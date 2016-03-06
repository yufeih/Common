namespace System
{
    using System.Text;
    using Xunit;

    public class Murmur3Test
    {
        [Theory]
        [InlineData("murmur", 777, 3048996684)]
        public void murmur_32(string text, uint seed, uint hash)
        {
            Assert.Equal(hash, Murmur3.Hash32(Encoding.ASCII.GetBytes(text), seed));
        }

        [Theory]
        [InlineData("murmur", 777, 12697776169225239660, 120866311087417157)]
        [InlineData("hello, world! This is a really long text block!", uint.MaxValue, 10459812056865126164, 11268304996813922978)]
        public void murmur_128(string text, uint seed, ulong hashA, ulong hashB)
        {
            ulong a, b;
            Murmur3.Hash128(Encoding.ASCII.GetBytes(text), seed, out a, out b);
            Assert.Equal(hashA, a);
            Assert.Equal(hashB, b);
        }
    }
}
