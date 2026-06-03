using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order;

public sealed class MakeReservationCommand : IntegrationCommand
{
    public required Guid OrderId { get; init; }
    public required IReadOnlyList<OrderItem> Items { get; init; }
}
