using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Finance.Domain.Aggregates.Account.Commands;

public sealed record CreateAccountCommand : ICommand
{
    public required Guid OrderId { get; init; }
    public required string BuyerId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public string? PaymentFrequency { get; init; }
    public required string TenantId { get; init; }
    public required string ActionUserId { get; init; }
    public required string ActionUserType { get; init; }
}
