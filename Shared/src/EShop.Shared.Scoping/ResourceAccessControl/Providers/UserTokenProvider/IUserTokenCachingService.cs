using EShop.Shared.Contracts.Services.Identity.Auth;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface IUserTokenCachingService
{
    Task<Response.AuthenticatedResponse?> TryGetTokenAsync(string userId);

    Task AddTokenAsync(string userId, Response.AuthenticatedResponse token);

    Task RemoveCacheAsync(string userId);
}