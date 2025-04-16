using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public class OrganizationContextCachingService : IOrganizationContextCachingService
{
    private readonly IRedisCachingAsyncProvider<OrganizationContext> _redisCachingAsyncProvider;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public OrganizationContextCachingService(
        IRedisCachingAsyncProvider<OrganizationContext> redisCachingAsyncProvider,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingAsyncProvider = redisCachingAsyncProvider;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }


    public Task AddValue(string organizationId, OrganizationContext organizationContext)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetOrganizationContextCacheKey(organizationId);
        return _redisCachingAsyncProvider.AddAsync(
            cacheKey,
            organizationContext,
            new DistributedCacheEntryOptions { SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration() });
    }

    public async Task<OrganizationContext?> GetValue(string organizationId)
    {
        var cachedOrganizationContext = await _redisCachingAsyncProvider.GetAsync(
            OrganizationContextCacheKeyProvider.GetOrganizationContextCacheKey(organizationId));
        return cachedOrganizationContext;
    }

    public async Task RemoveValue(string organizationId)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetOrganizationContextCacheKey(organizationId);
        await _redisCachingAsyncProvider.ClearAsync(cacheKey);
    }
}