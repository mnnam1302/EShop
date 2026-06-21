using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Order.Domain.Commands;

public sealed class AcceptOrderCommand : ICommand
{
    public required Guid OrderId { get; init; }
}
