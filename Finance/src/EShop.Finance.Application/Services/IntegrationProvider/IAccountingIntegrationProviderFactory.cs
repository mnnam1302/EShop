namespace EShop.Finance.Application.Services.IntegrationProvider;

/// <summary>
/// Resolves the <see cref="IAccountingIntegrationProvider"/> for a tenant (named-strategy registry).
/// </summary>
public interface IAccountingIntegrationProviderFactory
{
    Task<IAccountingIntegrationProvider> Create(string tenantId, CancellationToken cancellationToken);

    IAccountingIntegrationProvider GetByName(string providerType);
}
