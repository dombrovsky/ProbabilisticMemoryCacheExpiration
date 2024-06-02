namespace ProbabilisticMemoryCacheExpiration
{
#if NET8_0_OR_GREATER
    using Microsoft.Extensions.Caching.Memory;

    public class ProbabilisticExpirationMemoryCacheStatistics : MemoryCacheStatistics
    {
        /// <summary>
        /// Gets the total number of cache misses due to probabilistic expiration.
        /// </summary>
        public long TotalProbabilisticExpirationMisses { get; init; }
    }
#endif
}
