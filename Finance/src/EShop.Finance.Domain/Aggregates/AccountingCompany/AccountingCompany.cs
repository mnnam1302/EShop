using System.ComponentModel.DataAnnotations;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Finance.Domain.Aggregates.AccountingCompany;

public class AccountingCompany : AggregateRoot<Guid>, IScoped, IDateTracking
{
    [MaxLength(ModelConstants.ShortText)]
    public required string ProviderType { get; set; }

    public string? YamlConfiguration { get; set; }

    public string? EncryptedConnectionDetails { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string TenantId { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public required string Scope { get; set; }

    public static AccountingCompany CreateDefault(string tenantId)
    {
        return new AccountingCompany
        {
            Id = Guid.NewGuid(),
            ProviderType = AccountingProviderNames.None,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            TenantId = tenantId,
            Scope = tenantId,
        };
    }

    public void Configure(string providerType, string? yamlConfiguration)
    {
        ProviderType = providerType;
        YamlConfiguration = yamlConfiguration;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetConnectionDetails(string? encryptedConnectionDetails)
    {
        EncryptedConnectionDetails = encryptedConnectionDetails;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }
}
