using EShop.Shared.Cache.Providers;
using EShop.Shared.Contracts.Services.Identity.Auth;

namespace EShop.Testing.JsonApiApplication;

public class TestUserTokenProvider : IRedisCachingProvider<Response.AuthenticatedResponse>
{
    private readonly Dictionary<string, Response.AuthenticatedResponse> userToken = new();

    public void AddValue(string cacheKey, Response.AuthenticatedResponse value)
    {
        if (userToken.ContainsKey(cacheKey))
        {
            this.ClearCache(cacheKey);
        }

        userToken.Add(cacheKey, value);
    }

    public void ClearCache(string cacheKey)
    {
        userToken.Clear();
    }

    public Response.AuthenticatedResponse? GetValue(string cacheKey)
    {
        var userIdKey = cacheKey.ToLower();

        return userToken.ContainsKey(userIdKey)
                ? userToken[userIdKey]
                : null;
    }
}