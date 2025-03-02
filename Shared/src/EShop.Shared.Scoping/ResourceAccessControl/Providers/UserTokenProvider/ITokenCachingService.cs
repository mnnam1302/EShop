using EShop.Shared.Contracts.Services.Identity.Auth;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface ITokenCachingService
{
    Task<Response.AuthenticatedResponse?> TryGetTokenAsync(string userId);

    Task AddTokenAsync(string userId, Response.AuthenticatedResponse token);

    Task RemoveCacheAsync(string userId);
}