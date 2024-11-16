using EShop.Shared.Contracts.Services.Identity.Auth;

namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface ITokenCachingService
{
    bool TryGetToken(string userId, out Response.AuthenticatedResponse token);

    void AddToken(string userId, Response.AuthenticatedResponse token);

    void RemoveCache(string userId);
}