using EShop.Shared.Contracts.Services.Identity;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EShop.Shared.Cache;

public class TokenRedisCachingService : ITokenCachingService
{
    private readonly IRedisCachingProvider<Response.AuthenticatedResponse> _redisCachingProvider;
    private readonly ILogger<TokenRedisCachingService> _logger;

    public TokenRedisCachingService(
        IRedisCachingProvider<Response.AuthenticatedResponse> redisCachingProvider,
        ILogger<TokenRedisCachingService> logger)
    {
        _redisCachingProvider = redisCachingProvider;
        _logger = logger;
    }

    public void AddToken(string userId, Response.AuthenticatedResponse token)
    {
        _redisCachingProvider.AddValue(UserTokenCacheKeyProvider.GetCacheKey(userId), token);
    }

    public void RemoveCache(string userId)
    {
        _redisCachingProvider.ClearCache(UserTokenCacheKeyProvider.GetCacheKey(userId));
    }

    public bool TryGetToken(string userId, out Response.AuthenticatedResponse token)
    {
        token = new Response.AuthenticatedResponse();
        try
        {
            var cachedToken = _redisCachingProvider.GetValue(UserTokenCacheKeyProvider.GetCacheKey(userId));
            token = cachedToken ?? token;
            return token != null && cachedToken?.UserId != "";
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection exception '{FailureType}' while retrieving cached permission for user '{userId}'", ex.FailureType, userId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while retrieving cached permission for user '{userId}'", userId);
        }
        return false;
    }
}