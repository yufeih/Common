namespace System
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class IdentifierIdTest
    {
        [Fact]
        public void equals()
        {
            Assert.Equal((IdentifierId)"asdf", (IdentifierId)"asdf");
            Assert.Equal((IdentifierId)"Asdf", (IdentifierId)"asdF");
            Assert.NotEqual((IdentifierId)"Asdf", (IdentifierId)"1234");
        }

        [Fact]
        public void sorted_order()
        {
            var strs = new List<string>
            {
                "",
                "a",
                "aa",
                "A9",
                "Abcdefghijklmnopqrst",
                new string('9', 24),
            };

            var ids = strs.Select(str => new IdentifierId(str)).ToList();

            var names = ids.Select(id => id.ToString()).ToArray();

            Assert.Equal(strs.Select(x => x.ToLower()), names);
        }

        [Theory]
        [InlineData("a ")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaa")]
        [InlineData("!")]
        public void invalid(string str)
        {
            Assert.ThrowsAny<Exception>(() => new IdentifierId(str));
        }
    }
}
