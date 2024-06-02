namespace ProbabilisticMemoryCacheExpiration;

using Microsoft.Extensions.Internal;

public class ProbabilisticExpirationMemoryCacheOptions
{
    public ISystemClock? SystemClock { get; init; }

    public TimeSpan RemainingTtlThreshold { get; init; }

#if NET6_0_OR_GREATER
    public Random Random { get; init; } = Random.Shared;
#else
    public required Random Random { get; init; }
#endif
}