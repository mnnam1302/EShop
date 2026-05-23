using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.Cache.Services;

public sealed class UserTokenRedisCachingService : IUserTokenCachingService
{
    private readonly IRedisCachingProvider<TokenAuthentication> _redisCachingService;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;
    private readonly ILogger<UserTokenRedisCachingService> _logger;

    public UserTokenRedisCachingService(
        IRedisCachingProvider<TokenAuthentication> redisCachingService,
        CachedRemoteConfiguration cachedRemoteConfiguration,
        ILogger<UserTokenRedisCachingService> logger)
    {
        _redisCachingService = redisCachingService;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
        _logger = logger;
    }

    public async Task<TokenAuthentication?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try new scoped cache key first
            var cachedToken = await _redisCachingService.GetAsync(UserTokenCacheKeyProvider.GetCacheKey(userId), cancellationToken);

            // Fallback to legacy key for migration compatibility
            if (cachedToken is null)
            {
                cachedToken = await _redisCachingService.GetAsync(UserTokenCacheKeyProvider.GetLegacyCacheKey(userId), cancellationToken);

                if (cachedToken is not null)
                {
                    _logger.LogInformation("Found token in legacy cache key for user '{UserId}', consider token refresh for migration", userId);
                }
            }

            return cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve cached token for user '{UserId}' from Redis", userId);
            return null;
        }
    }

    public async Task AddAsync(string userId, TokenAuthentication token, CancellationToken cancellationToken = default)
    {
        var cacheKey = UserTokenCacheKeyProvider.GetCacheKey(userId);
        await _redisCachingService.AddAsync(cacheKey, token, new DistributedCacheEntryOptions
        {
            SlidingExpiration = _cachedRemoteConfiguration.GetSlidingExpiration()
        }, cancellationToken);
    }

    public async Task RemoveAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = UserTokenCacheKeyProvider.GetCacheKey(userId);
        await _redisCachingService.RemoveAsync(cacheKey, cancellationToken);
    }
}