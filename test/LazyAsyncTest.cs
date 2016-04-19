namespace System
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    
    public class LazyAsyncTest
    {
        [Fact]
        public async Task concurrent_lazy_requests()
        {
            for (var i = 0; i < 100; i++)
            {
                var n = 0;
                var lazy = new Lazy<Task<int>>(async () =>
                {
                    await Task.Delay(10);
                    return Interlocked.Increment(ref n);
                });

                var bag = new ConcurrentBag<Task<int>>();
                Parallel.For(0, 1000, j =>
                {
                    bag.Add(lazy.Value);
                });

                var results = await Task.WhenAll(bag);

                for (int nn = 0; nn < 1000; nn++)
                {
                    Assert.Equal(1, results[nn]);
                }
            }
        }

        [Fact]
        public async Task retry_on_failure()
        {
            var n = 0;
            var lazy = new Lazy<Task<int>>(() =>
            {
                n++;
                throw new NotImplementedException();
            });

            try { await lazy.Value; } catch (NotImplementedException) { }
            try { await lazy.Value; } catch (NotImplementedException) { }

            Assert.Equal(1, n);
        }
    }
}
