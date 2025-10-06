using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public sealed class UserTokenRedisCachingService : IUserTokenCachingService
{
    private readonly IRedisCachingProvider<TokenAuthenticationCaching> _redisCachingService;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public UserTokenRedisCachingService(
        IRedisCachingProvider<TokenAuthenticationCaching> redisCachingService,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingService = redisCachingService;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task<TokenAuthenticationCaching?> TryGetTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cachedToken = await _redisCachingService.GetAsync(UserTokenCacheKeyProvider.GetCacheKey(userId), cancellationToken);

        if (cachedToken is null)
        {
            throw new BadRequestException($"Invalid cached token for user '{userId}'");
        }

        return cachedToken;
    }

    public async Task AddTokenAsync(string userId, TokenAuthenticationCaching token, CancellationToken cancellationToken = default)
    {
        var key = UserTokenCacheKeyProvider.GetCacheKey(userId);
        await _redisCachingService.AddAsync(key, token, new DistributedCacheEntryOptions
        {
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration()
        }, cancellationToken);
    }

    public async Task RemoveCacheAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _redisCachingService.ClearAsync(UserTokenCacheKeyProvider.GetCacheKey(userId), cancellationToken);
    }
}