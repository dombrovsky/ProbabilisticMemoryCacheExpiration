namespace ProbabilisticMemoryCacheExpiration;

using Microsoft.Extensions.Caching.Memory;

public interface IMemoryCacheWithProbabilisticExpiration : IMemoryCache
{
#if NET8_0_OR_GREATER
    new ProbabilisticExpirationMemoryCacheStatistics? GetCurrentStatistics();
#endif
}