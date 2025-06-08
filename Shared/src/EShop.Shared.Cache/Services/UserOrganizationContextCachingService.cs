using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.Extensions.Caching.Distributed;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Shared.Cache.Services;

public class UserOrganizationContextCachingService : IUserOrganizationContextCachingService
{
    private readonly IRedisCachingAsyncProvider<UserOrganizationContext> _redisCachingAsyncProvider;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public UserOrganizationContextCachingService(
        IRedisCachingAsyncProvider<UserOrganizationContext> redisCachingAsyncProvider,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingAsyncProvider = redisCachingAsyncProvider;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task AddValue(string userId, string userType, UserOrganizationContext userOrganizationContext)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetUserOrganizationContextCacheKey(userId, userType);
        await _redisCachingAsyncProvider.AddAsync(
            cacheKey,
            userOrganizationContext,
            new DistributedCacheEntryOptions { SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration() });
    }

    public async Task<UserOrganizationContext?> GetValue(string userId, string userType)
    {
        var cachedUserOrganizationContext = await _redisCachingAsyncProvider.GetAsync(
            OrganizationContextCacheKeyProvider.GetUserOrganizationContextCacheKey(userId, userType));

        return cachedUserOrganizationContext;
    }

    public async Task RemoveValue(string userId, string userType)
    {
        var cacheKey = OrganizationContextCacheKeyProvider.GetUserOrganizationContextCacheKey(userId, userType);
        await _redisCachingAsyncProvider.ClearAsync(cacheKey);
    }
}