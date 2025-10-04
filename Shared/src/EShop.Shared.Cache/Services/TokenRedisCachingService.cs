using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services;

public sealed class TokenRedisCachingService : IUserTokenCachingService
{
    private readonly IRedisCachingProvider<TokenAuthenticationCaching> _redisCachingService;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public TokenRedisCachingService(
        IRedisCachingProvider<TokenAuthenticationCaching> redisCachingService,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingService = redisCachingService;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task<TokenAuthenticationCaching?> TryGetTokenAsync(string userId)
    {
        var cachedToken = await _redisCachingService.GetAsync(UserTokenCacheKeyProvider.GetCacheKey(userId));

        if (cachedToken is null)
        {
            throw new BadRequestException($"Invalid cached token for user '{userId}'");
        }

        return cachedToken;
    }

    public async Task AddTokenAsync(string userId, TokenAuthenticationCaching token)
    {
        var key = UserTokenCacheKeyProvider.GetCacheKey(userId);
        await _redisCachingService.AddAsync(key, token, new DistributedCacheEntryOptions
        {
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingTokenExpiration()
        });
    }

    public async Task RemoveCacheAsync(string userId)
    {
        await _redisCachingService.ClearAsync(UserTokenCacheKeyProvider.GetCacheKey(userId));
    }
}