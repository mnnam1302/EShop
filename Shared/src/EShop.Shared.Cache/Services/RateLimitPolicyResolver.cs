using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Caching.Memory;

namespace EShop.Shared.Cache.Services;

public interface IRateLimitPolicyResolver
{
    Task<CachedRateLimitPolicy?> GetPolicy(string tenantId, CancellationToken cancellationToken = default);

    bool TryGetCachedPolicy(string tenantId, out CachedRateLimitPolicy? policy);
}

public sealed class RateLimitPolicyResolver : IRateLimitPolicyResolver
{
    private static readonly TimeSpan L1CacheDuration = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly IMemoryCache _memoryCache;
    private readonly IRateLimitPolicyCachingService _rateLimitPolicyCachingService;
    private readonly RateLimitPolicyHttpClient _rateLimitPolicyHttpClient;
    private readonly IDistributedLock _distributedLock;

    public RateLimitPolicyResolver(
        IMemoryCache memoryCache,
        IRateLimitPolicyCachingService rateLimitPolicyCachingService,
        RateLimitPolicyHttpClient rateLimitPolicyHttpClient,
        IDistributedLock distributedLock)
    {
        _memoryCache = memoryCache;
        _rateLimitPolicyCachingService = rateLimitPolicyCachingService;
        _rateLimitPolicyHttpClient = rateLimitPolicyHttpClient;
        _distributedLock = distributedLock;
    }

    public async Task<CachedRateLimitPolicy?> GetPolicy(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("TenantId is required", nameof(tenantId));
        }

        var l1Key = GetL1CacheKey(tenantId);

        if (_memoryCache.TryGetValue<CachedRateLimitPolicy>(l1Key, out var l1Cached) && l1Cached is not null)
        {
            return l1Cached;
        }

        var redisCached = await _rateLimitPolicyCachingService.GetRateLimitPolicy(tenantId, cancellationToken);
        if (redisCached is not null)
        {
            SetL1Cache(l1Key, redisCached);
            return redisCached;
        }

        using var lockHandle = await _distributedLock.TryAcquireAsync($"ratelimit-policy:{tenantId}", LockExpiration, cancellationToken);

        if (lockHandle is not null)
        {
            redisCached = await _rateLimitPolicyCachingService.GetRateLimitPolicy(tenantId, cancellationToken);
            if (redisCached is not null)
            {
                SetL1Cache(l1Key, redisCached);
                return redisCached;
            }

            var fetched = await _rateLimitPolicyHttpClient.GetRateLimitPolicyAsync(tenantId, cancellationToken);
            if (fetched is not null)
            {
                await _rateLimitPolicyCachingService.AddRateLimitPolicy(tenantId, fetched, cancellationToken);
                SetL1Cache(l1Key, fetched);
            }

            return fetched;
        }

        await Task.Delay(LockRetryDelay, cancellationToken);

        var afterWait = await _rateLimitPolicyCachingService.GetRateLimitPolicy(tenantId, cancellationToken);
        if (afterWait is not null)
        {
            SetL1Cache(l1Key, afterWait);
        }

        return afterWait;
    }

    public bool TryGetCachedPolicy(string tenantId, out CachedRateLimitPolicy? policy)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("TenantId is required", nameof(tenantId));
        }

        return _memoryCache.TryGetValue(GetL1CacheKey(tenantId), out policy);
    }

    private void SetL1Cache(string key, CachedRateLimitPolicy policy)
    {
        _memoryCache.Set(key, policy, L1CacheDuration);
    }

    private static string GetL1CacheKey(string tenantId) => $"ratelimit-policy:l1:{tenantId}";
}
