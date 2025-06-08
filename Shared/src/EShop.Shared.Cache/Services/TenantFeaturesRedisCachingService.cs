using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public class TenantFeaturesRedisCachingService : ITenantFeaturesCachingService
{
    private readonly IRedisCachingAsyncProvider<string[]> _redisCachingService;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public TenantFeaturesRedisCachingService(IRedisCachingAsyncProvider<string[]> redisCachingService, CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingService = redisCachingService;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task<string[]> GetTenantFeatures(string tenantId, CancellationToken cancellationToken = default)
    {
        var cacheTenantFeatures = await _redisCachingService.GetAsync(TenantFeaturesCacheKeyProvider.GetCacheKey(tenantId), cancellationToken);
        if (cacheTenantFeatures is null)
        {
            return [];
        }
        return cacheTenantFeatures;
    }

    public async Task AddTenantFeatures(string tenantId, string[] features, CancellationToken cancellationToken = default)
    {
        await _redisCachingService.AddAsync(
            TenantFeaturesCacheKeyProvider.GetCacheKey(tenantId),
            features,
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration()
            },
            cancellationToken);
    }

    public async Task RemoveTenantFeatures(string tenantId, CancellationToken cancellationToken = default)
    {
        await _redisCachingService.ClearAsync(TenantFeaturesCacheKeyProvider.GetCacheKey(tenantId), cancellationToken);
    }
}