using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;

public sealed record ConfigureAccountingCompanyCommand : ICommand
{
    public required string TenantId { get; init; }
    public required string ProviderType { get; init; }
    public string? YamlConfiguration { get; init; }
}
