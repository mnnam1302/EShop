namespace EShop.Finance.Application.Services.IntegrationProvider;

public interface IAccountingIntegrationProviderFactory
{
    Task<IAccountingIntegrationProvider> Create(string tenantId, CancellationToken cancellationToken);

    IAccountingIntegrationProvider GetByName(string providerType);
}
