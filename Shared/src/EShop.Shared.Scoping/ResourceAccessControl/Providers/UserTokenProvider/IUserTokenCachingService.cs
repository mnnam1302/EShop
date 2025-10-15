namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;

public interface IUserTokenCachingService
{
    Task<TokenAuthentication?> TryGetTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task AddTokenAsync(string userId, TokenAuthentication token, CancellationToken cancellationToken = default);
    Task RemoveCacheAsync(string userId, CancellationToken cancellationToken = default);
}