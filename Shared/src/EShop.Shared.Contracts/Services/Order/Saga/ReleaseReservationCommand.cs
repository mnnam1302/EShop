using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order.Saga;

/// <summary>
/// Sent by the saga to the Inventory service to release a previously created
/// reservation (compensation path).
/// </summary>
public sealed class ReleaseReservationCommand : IntegrationCommand
{
    public required Guid OrderId { get; init; }

    public required Guid ReservationId { get; init; }
}
