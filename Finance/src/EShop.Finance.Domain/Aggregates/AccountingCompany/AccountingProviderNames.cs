namespace EShop.Finance.Domain.Aggregates.AccountingCompany;

/// <summary>
/// Well-known accounting provider type identifiers. A tenant's <see cref="AccountingCompany.ProviderType"/>
/// is matched against the registered <c>IAccountingIntegrationProvider.Name</c> values.
/// </summary>
public static class AccountingProviderNames
{
    public const string None = "None";
    public const string GenericHttp = "GenericHttp";
}
