using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Order.Domain.Commands;

public sealed class RejectOrderCommand : ICommand
{
    public required Guid OrderId { get; init; }
    public required string Reason { get; init; }
}
