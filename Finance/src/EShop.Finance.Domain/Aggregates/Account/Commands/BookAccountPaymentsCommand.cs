using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Finance.Domain.Aggregates.Account.Commands;

public sealed record BookAccountPaymentsCommand : ICommand
{
    public required Guid AccountId { get; init; }
}
