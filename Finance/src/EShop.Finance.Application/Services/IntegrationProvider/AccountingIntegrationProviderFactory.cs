using EShop.Finance.Domain.Abstractions;
using EShop.Shared.DomainTools.Exceptions;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Application.Services.IntegrationProvider;

internal sealed class AccountingIntegrationProviderFactory : IAccountingIntegrationProviderFactory
{
    private readonly IEnumerable<IAccountingIntegrationProvider> providers;
    private readonly IAccountingCompanyRepository accountingCompanies;
    private readonly ILogger<AccountingIntegrationProviderFactory> logger;

    public AccountingIntegrationProviderFactory(
        IEnumerable<IAccountingIntegrationProvider> providers,
        IAccountingCompanyRepository accountingCompanies,
        ILogger<AccountingIntegrationProviderFactory> logger)
    {
        this.providers = providers;
        this.accountingCompanies = accountingCompanies;
        this.logger = logger;
    }

    public async Task<IAccountingIntegrationProvider> Create(string tenantId, CancellationToken cancellationToken)
    {
        var accountingCompany = await accountingCompanies.FindByTenantIdAsync(tenantId, cancellationToken: cancellationToken);
        if (accountingCompany is null)
        {
            throw new NotFoundException($"No accounting company for tenant '{tenantId}'.");
        }

        logger.LogDebug("Resolved accounting provider '{ProviderType}' for tenant {TenantId}.", accountingCompany.ProviderType, tenantId);
        return GetByName(accountingCompany.ProviderType);
    }

    public IAccountingIntegrationProvider GetByName(string providerType)
    {
        var provider = providers.SingleOrDefault(p => p.Name.Equals(providerType, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            throw new InvalidOperationException($"No accounting integration provider is registered for type '{providerType}'.");
        }

        return provider;
    }
}
