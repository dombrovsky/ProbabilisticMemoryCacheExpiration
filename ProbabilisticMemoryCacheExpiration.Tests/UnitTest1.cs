using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace MemoryCache.ProbabilisticExpiration.Test
{
    using ProbabilisticMemoryCacheExpiration;

    public class Tests
    {
        private Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = null!;
        private ProbabilisticExpirationMemoryCacheDecorator _sut = null!;
        [SetUp]
        public void Setup()
        {
            _memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
            _sut = new ProbabilisticExpirationMemoryCacheDecorator(_memoryCache, new ProbabilisticExpirationMemoryCacheOptions { RemainingTtlThreshold = TimeSpan.FromMinutes(1) });
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
            _memoryCache.Dispose();
        }

        [Test]
        public void Test1()
        {
            const int uniqueKeys = 100;

            // set new keys
            for (int i = 0; i < uniqueKeys; i++)
            {
                _sut.Set($"key{i}", new byte[10], TimeSpan.FromMinutes(1.5));
            }

            // read keys
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromMinutes(2))
            {
                var i = Random.Shared.Next(0, uniqueKeys - 1);
                var cachedValue = _sut.Get($"key{i}");
                if (cachedValue != null)
                {
                    //Console.WriteLine($"{i}: HIT");
                }
                else
                {
                    //Console.WriteLine($"{i}: MISS");
                    _sut.Set($"key{i}", new byte[10], TimeSpan.FromMinutes(1.5));
                }
            }

            var statistics = _sut.GetCurrentStatistics();
        }
    }
}