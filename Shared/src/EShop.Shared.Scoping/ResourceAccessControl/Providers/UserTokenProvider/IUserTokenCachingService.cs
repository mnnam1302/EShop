namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface IUserTokenCachingService
{
    Task<TokenAuthenticationCaching?> TryGetTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task AddTokenAsync(string userId, TokenAuthenticationCaching token, CancellationToken cancellationToken = default);
    Task RemoveCacheAsync(string userId, CancellationToken cancellationToken = default);
}