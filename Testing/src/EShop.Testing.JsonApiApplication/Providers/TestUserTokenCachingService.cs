using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;

namespace EShop.Testing.JsonApiApplication.Providers;

public class TestUserTokenCachingService : IUserTokenCachingService
{
    private readonly Dictionary<string, TokenAuthentication> userTokens = new();

    Task<TokenAuthentication?> IUserTokenCachingService.GetAsync(string userId, CancellationToken cancellationToken)
    {
        userTokens.TryGetValue(userId, out var value);
        return Task.FromResult<TokenAuthentication?>(value);
    }

    public Task AddAsync(string userId, TokenAuthentication token, CancellationToken cancellationToken = default)
    {
        if (userTokens.ContainsKey(userId))
        {
            userTokens[userId] = token;
        }
        else
        {
            userTokens.Add(userId, token);
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string userId, CancellationToken cancellationToken = default)
    {
        userTokens.Remove(userId);
        return Task.CompletedTask;
    }
}