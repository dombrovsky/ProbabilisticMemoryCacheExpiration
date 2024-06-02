namespace ProbabilisticMemoryCacheExpiration;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

internal sealed class CacheEntryWithProbabilisticExpirationWrapper : ICacheEntry
{
    private readonly ICacheEntry _baseCacheEntry;

    public CacheEntryWithProbabilisticExpirationWrapper(ICacheEntry baseCacheEntry)
    {
        _baseCacheEntry = baseCacheEntry;
    }

    public void Dispose()
    {
        _baseCacheEntry.Dispose();
    }

    public object Key => _baseCacheEntry.Key;

    public object? Value
    {
        get => _baseCacheEntry.Value;
        set => _baseCacheEntry.Value = value;
    }

    public DateTimeOffset? AbsoluteExpiration
    {
        get => _baseCacheEntry.AbsoluteExpiration;
        set => _baseCacheEntry.AbsoluteExpiration = value;
    }

    public TimeSpan? AbsoluteExpirationRelativeToNow
    {
        get => _baseCacheEntry.AbsoluteExpirationRelativeToNow;
        set => _baseCacheEntry.AbsoluteExpirationRelativeToNow = value;
    }

    public TimeSpan? SlidingExpiration
    {
        get => _baseCacheEntry.SlidingExpiration;
        set => _baseCacheEntry.SlidingExpiration = value;
    }

    public IList<IChangeToken> ExpirationTokens => _baseCacheEntry.ExpirationTokens;

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => _baseCacheEntry.PostEvictionCallbacks;

    public CacheItemPriority Priority
    {
        get => _baseCacheEntry.Priority;
        set => _baseCacheEntry.Priority = value;
    }

    public long? Size
    {
        get => _baseCacheEntry.Size;
        set => _baseCacheEntry.Size = value;
    }

    internal DateTime LastAccessed { get; set; }
}