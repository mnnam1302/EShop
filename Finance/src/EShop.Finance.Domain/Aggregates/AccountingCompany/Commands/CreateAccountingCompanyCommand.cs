using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;

public sealed record CreateAccountingCompanyCommand : ICommand
{
    public required string TenantId { get; init; }
}
