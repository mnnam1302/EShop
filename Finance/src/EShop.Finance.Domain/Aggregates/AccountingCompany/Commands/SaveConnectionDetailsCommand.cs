using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;

public sealed record SaveConnectionDetailsCommand : ICommand
{
    public required string TenantId { get; init; }
    public required IReadOnlyDictionary<string, string?> ConnectionDetails { get; init; }
}
