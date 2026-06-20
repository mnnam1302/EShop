namespace EShop.Shared.Contracts.Services.Inventory;

/// <summary>Published by the background expiry job when a reservation TTL elapses without confirmation.</summary>
public sealed class ReservationExpired : InventoryIntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required Guid ReservationId { get; init; }

    public required DateTimeOffset ExpiredAt { get; init; }
}
