using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public sealed class OrganizationContextCachingService : IOrganizationContextCachingService
{
    private readonly IRedisCachingProvider<OrganizationContext> _redisCachingAsyncProvider;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public OrganizationContextCachingService(
        IRedisCachingProvider<OrganizationContext> redisCachingAsyncProvider,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingAsyncProvider = redisCachingAsyncProvider;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public Task AddValue(string organizationId, OrganizationContext organizationContext, CancellationToken cancellationToken = default)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetOrganizationContextCacheKey(organizationId);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingExpiration()
        };

        return _redisCachingAsyncProvider.AddAsync(cacheKey, organizationContext, cacheOptions, cancellationToken);
    }

    public async Task<OrganizationContext?> GetValue(string organizationId, CancellationToken cancellationToken = default)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetOrganizationContextCacheKey(organizationId);

        var cachedOrganizationContext = await _redisCachingAsyncProvider.GetAsync(cacheKey, cancellationToken);

        return cachedOrganizationContext;
    }

    public async Task RemoveValue(string organizationId, CancellationToken cancellationToken = default)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetOrganizationContextCacheKey(organizationId);

        await _redisCachingAsyncProvider.RemoveAsync(cacheKey, cancellationToken);
    }
}