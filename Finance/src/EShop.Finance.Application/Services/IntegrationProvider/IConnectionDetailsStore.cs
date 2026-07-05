namespace EShop.Finance.Application.Services.IntegrationProvider;

public interface IConnectionDetailsStore
{
    Task<IReadOnlyDictionary<string, string?>?> GetConnectionDetails(string tenantId, CancellationToken cancellationToken);

    Task SaveConnectionDetails(string tenantId, IReadOnlyDictionary<string, string?> connectionDetails, CancellationToken cancellationToken);
}
