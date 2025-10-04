namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface IUserTokenCachingService
{
    Task<AuthenticationCaching?> TryGetTokenAsync(string userId);
    Task AddTokenAsync(string userId, AuthenticationCaching token);
    Task RemoveCacheAsync(string userId);
}