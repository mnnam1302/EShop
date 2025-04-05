using EShop.Shared.Cache.Providers;
using EShop.Shared.Contracts.Services.Identity.Auth;

namespace EShop.Testing.JsonApiApplication;

public class TestUserTokenProvider : IRedisCachingProvider<Response.AuthenticatedResponse>
{
    private readonly Dictionary<string, Response.AuthenticatedResponse> userTokens = new();

    public void AddValue(string cacheKey, Response.AuthenticatedResponse value)
    {
        if (userTokens.ContainsKey(cacheKey))
        {
            this.ClearCache(cacheKey);
        }

        userTokens.Add(cacheKey, value);
    }

    public void ClearCache(string cacheKey)
    {
        userTokens.Clear();
    }

    public Response.AuthenticatedResponse? GetValue(string cacheKey)
    {
        var userIdKey = cacheKey.ToLower();

        return userTokens.ContainsKey(userIdKey) ? userTokens[userIdKey] : null;
    }
}