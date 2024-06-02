namespace ProbabilisticMemoryCacheExpiration;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

public class ProbabilisticExpirationMemoryCacheDecorator : IMemoryCacheWithProbabilisticExpiration, IMemoryCache
{
    private readonly IMemoryCache _baseMemoryCache;
    private readonly ISystemClock _systemClock;
    private readonly TimeSpan _remainingTtlThreshold;
    private readonly Random _random;
    private readonly ConcurrentDictionary<object, CacheEntryWithProbabilisticExpirationWrapper> _cacheEntries;

    private long _totalProbabilisticExpirationMisses;

    public ProbabilisticExpirationMemoryCacheDecorator(
        IMemoryCache baseMemoryCache,
        ProbabilisticExpirationMemoryCacheOptions options)
    {
        Argument.NotNull(baseMemoryCache);
        Argument.NotNull(options);

        _baseMemoryCache = baseMemoryCache;
        _systemClock = options.SystemClock ?? new SystemClock();
        _remainingTtlThreshold = options.RemainingTtlThreshold;
        _random = options.Random;
        _cacheEntries = new ConcurrentDictionary<object, CacheEntryWithProbabilisticExpirationWrapper>();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Not security-sensitive")]
    public bool TryGetValue(object key, out object? value)
    {
        if (_baseMemoryCache.TryGetValue(key, out value))
        {
            if (_cacheEntries.TryGetValue(key, out var entry))
            {
                var now = _systemClock.UtcNow.UtcDateTime;
                var ttl = GetTtl(entry, now);

                if (ttl.HasValue && ttl < _remainingTtlThreshold)
                {
                    var probabilityFactor = 1 - (ttl.Value.TotalSeconds / _remainingTtlThreshold.TotalSeconds);
                    if (_random.NextDouble() < probabilityFactor)
                    {
                        Interlocked.Increment(ref _totalProbabilisticExpirationMisses);
                        value = null;
                        return false;
                    }
                }

                entry.LastAccessed = now;
            }

            return true;
        }

        value = null;
        return false;
    }

    public ICacheEntry CreateEntry(object key)
    {
        return _cacheEntries.AddOrUpdate(key, AddEntry, UpdateValueFactory);

        CacheEntryWithProbabilisticExpirationWrapper UpdateValueFactory(object arg1, CacheEntryWithProbabilisticExpirationWrapper arg2)
        {
            return AddEntry(arg1);
        }

        CacheEntryWithProbabilisticExpirationWrapper AddEntry(object arg)
        {
            return new CacheEntryWithProbabilisticExpirationWrapper(
                _baseMemoryCache
                    .CreateEntry(key)
                    .RegisterPostEvictionCallback((k, value, reason, state) =>
                    {
#if NET6_0_OR_GREATER
                        _cacheEntries.Remove(k, out _);
#else
                        _cacheEntries.TryRemove(k, out _);
#endif
                    }));
        }
    }

    public void Remove(object key)
    {
        _baseMemoryCache.Remove(key);
    }

#if NET8_0_OR_GREATER
    public ProbabilisticExpirationMemoryCacheStatistics? GetCurrentStatistics()
    {
        var statistics = _baseMemoryCache.GetCurrentStatistics();
        if (statistics == null)
        {
            return null;
        }

        return new ProbabilisticExpirationMemoryCacheStatistics
        {
            CurrentEntryCount = statistics.CurrentEntryCount,
            CurrentEstimatedSize = statistics.CurrentEstimatedSize,
            TotalHits = statistics.TotalHits,
            TotalMisses = statistics.TotalMisses,
            TotalProbabilisticExpirationMisses = _totalProbabilisticExpirationMisses,
        };
    }

    MemoryCacheStatistics? IMemoryCache.GetCurrentStatistics()
    {
        return GetCurrentStatistics();
    }
#endif

    protected virtual void Dispose(bool disposing)
    {
    }

    private static TimeSpan? GetTtl(CacheEntryWithProbabilisticExpirationWrapper entry, DateTime utcNow)
    {
        if (entry.AbsoluteExpiration.HasValue)
        {
            return entry.AbsoluteExpiration - utcNow;
        }

        if (entry.SlidingExpiration.HasValue)
        {
            return entry.LastAccessed - utcNow + entry.SlidingExpiration;
        }

        return null;
    }
}