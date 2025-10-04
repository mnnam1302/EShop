namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface IUserTokenCachingService
{
    Task<AuthenticatedResult?> TryGetTokenAsync(string userId);
    Task AddTokenAsync(string userId, AuthenticatedResult token);
    Task RemoveCacheAsync(string userId);
}