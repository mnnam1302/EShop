namespace EShop.Shared.Authentication.Abstractions
{
    public interface IUserTokenCachingService
    {
        Task<TokenAuthentication?> GetAsync(string userId, CancellationToken cancellationToken = default);

        Task AddAsync(string userId, TokenAuthentication token, CancellationToken cancellationToken = default);

        Task RemoveAsync(string userId, CancellationToken cancellationToken = default);
    }
}