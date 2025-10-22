using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public sealed class UserOrganizationContextCachingService : IUserOrganizationContextCachingService
{
    private readonly IRedisCachingProvider<UserOrganizationContext> _redisCachingProvider;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public UserOrganizationContextCachingService(
        IRedisCachingProvider<UserOrganizationContext> redisCachingAsyncProvider,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingProvider = redisCachingAsyncProvider;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task AddValue(string userId, string userType, UserOrganizationContext userOrganizationContext, CancellationToken cancellationToken = default)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetUserOrganizationContextCacheKey(userId, userType);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingExpiration()
        };

        await _redisCachingProvider.AddAsync(cacheKey, userOrganizationContext, cacheOptions, cancellationToken);
    }

    public async Task<UserOrganizationContext?> GetValue(string userId, string userType, CancellationToken cancellationToken = default)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetUserOrganizationContextCacheKey(userId, userType);

        var cachedUserOrganizationContext = await _redisCachingProvider.GetAsync(cacheKey, cancellationToken);

        return cachedUserOrganizationContext;
    }

    public async Task RemoveValue(string userId, string userType, CancellationToken cancellationToken = default)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetUserOrganizationContextCacheKey(userId, userType);

        await _redisCachingProvider.ClearAsync(cacheKey, cancellationToken);
    }
}