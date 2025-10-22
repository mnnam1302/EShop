namespace EShop.Shared.Authentication.Abstractions
{
    public interface IRsaKeyCachingService
    {
        Task<RsaKeyPair?> GetAsync(string tenantId, CancellationToken cancellationToken);

        Task AddAsync(string tenantId, RsaKeyPair keyPair, CancellationToken cancellationToken);

        Task RemoveAsync(string tenantId, CancellationToken cancellationToken);
    }
}
