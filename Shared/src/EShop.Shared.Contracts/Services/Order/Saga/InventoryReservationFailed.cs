using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order.Saga;

public sealed class InventoryReservationFailed : IntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required string FailureReason { get; init; }
}
