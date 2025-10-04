namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface IUserTokenCachingService
{
    Task<TokenAuthenticationCaching?> TryGetTokenAsync(string userId);
    Task AddTokenAsync(string userId, TokenAuthenticationCaching token);
    Task RemoveCacheAsync(string userId);
}