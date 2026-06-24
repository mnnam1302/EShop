using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order.Saga;

public sealed class InventoryReserved : IntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required Guid ReservationId { get; init; }
}
