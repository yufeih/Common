namespace System
{
    using System.Diagnostics;
    using Xunit;

    public class Foo
    {
        public int Id;
        public string Name { get; set; }
        public string ApplicationName;
        public DateTime Time;
        public DateTime? NullableTime;
        public DateTime? NullableTime2 { get; set; }
        public StringComparison Enum { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTime Time3 { get; set; }

        public Foo Clone() => (Foo)MemberwiseClone();
    }
    
    public struct Bar
    {
        public int Id;
        public string Name { get; set; }
        public string ApplicationName;
        public DateTime Time;
        public DateTime? NullableTime;
        public DateTime? NullableTime2 { get; set; }
        public StringComparison Enum { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTime Time3 { get; set; }
    }
    
    public class ObjectHelperTest
    {
        [Fact]
        public void with()
        {
            var foo = new Foo { Id = 1, Name = "a" };
            var foo1 = foo.With(new Foo { Id = 2 }, nameof(Foo.Id));
            Assert.Equal(2, foo1.Id);
            Assert.Equal("a", foo1.Name);
            Assert.NotEqual(foo, foo1);

            var foo2 = foo.With(new Foo { Id = 3, Name = "b" }, nameof(Foo.Id), nameof(Foo.Name));
            Assert.Equal(3, foo2.Id);
            Assert.Equal("b", foo2.Name);
            Assert.NotEqual(foo, foo2);
        }

        [Fact]
        public void perf()
        {
            var foo = new Foo();
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 100 * 1000; i++)
            {
                ObjectHelper.Merge(new Foo(), foo);
                foo.Id++;
            }
            Console.WriteLine(sw.ElapsedMilliseconds);

            sw.Stop();
            sw = Stopwatch.StartNew();
            for (var i = 0; i < 100 * 1000; i++)
            {
                ObjectHelper.MemberwiseClone(foo);
                foo.Id++;
            }
            Console.WriteLine(sw.ElapsedMilliseconds);

            sw.Stop();
            sw = Stopwatch.StartNew();
            for (var i = 0; i < 100 * 1000; i++)
            {
                foo.Clone();
                foo.Id++;
            }
            Console.WriteLine(sw.ElapsedMilliseconds);

            var bar = new Bar();
            var barz = new Bar();
            sw.Stop();
            sw = Stopwatch.StartNew();
            for (var i = 0; i < 100 * 1000; i++)
            {
                barz = bar;
                barz.Id++;
            }
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}
