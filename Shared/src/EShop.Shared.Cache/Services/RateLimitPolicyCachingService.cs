using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public interface IRateLimitPolicyCachingService
{
    Task<CachedRateLimitPolicy?> GetRateLimitPolicy(string tenantId, CancellationToken cancellationToken = default);

    Task AddRateLimitPolicy(string tenantId, CachedRateLimitPolicy policy, CancellationToken cancellationToken = default);

    Task RemoveRateLimitPolicy(string tenantId, CancellationToken cancellationToken = default);
}

public sealed class RateLimitPolicyCachingService : IRateLimitPolicyCachingService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IRedisCachingProvider<CachedRateLimitPolicy> _redisCachingService;

    public RateLimitPolicyCachingService(IRedisCachingProvider<CachedRateLimitPolicy> redisCachingService)
    {
        _redisCachingService = redisCachingService;
    }

    public async Task<CachedRateLimitPolicy?> GetRateLimitPolicy(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _redisCachingService.GetAsync(RateLimitPolicyCacheKeyProvider.GetCacheKey(tenantId), cancellationToken);
    }

    public async Task AddRateLimitPolicy(string tenantId, CachedRateLimitPolicy policy, CancellationToken cancellationToken = default)
    {
        await _redisCachingService.AddAsync(
            RateLimitPolicyCacheKeyProvider.GetCacheKey(tenantId),
            policy,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration },
            cancellationToken);
    }

    public async Task RemoveRateLimitPolicy(string tenantId, CancellationToken cancellationToken = default)
    {
        await _redisCachingService.RemoveAsync(RateLimitPolicyCacheKeyProvider.GetCacheKey(tenantId), cancellationToken);
    }
}
