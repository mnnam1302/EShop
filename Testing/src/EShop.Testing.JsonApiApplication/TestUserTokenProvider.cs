using EShop.Shared.Cache.Providers;
using EShop.Shared.Contracts.Services.Identity.Auth;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Testing.JsonApiApplication;

public class TestUserTokenProvider : IRedisCachingAsyncProvider<Response.AuthenticatedResponse>
{
    private readonly Dictionary<string, Response.AuthenticatedResponse> userTokens = new();

    public async Task AddAsync(string cacheKey, Response.AuthenticatedResponse value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        if (userTokens.ContainsKey(cacheKey))
        {
            await this.ClearAsync(cacheKey);
        }

        userTokens.Add(cacheKey, value);
        await Task.CompletedTask;
    }

    public async Task ClearAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        userTokens.Clear();
        await Task.CompletedTask;
    }

    public async Task<Response.AuthenticatedResponse?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        var userIdKey = cacheKey.ToLower();

        userTokens.TryGetValue(userIdKey, out var value);
        return await Task.FromResult(value);
    }
}