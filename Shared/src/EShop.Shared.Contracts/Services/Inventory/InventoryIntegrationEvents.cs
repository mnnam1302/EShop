namespace EShop.Shared.Contracts.Services.Inventory;

/// <summary>Published by Inventory when all stock for an order has been reserved successfully.</summary>
public sealed class StockReserved : InventoryIntegrationEvent
{
    /// <summary>The order that triggered the reservation.</summary>
    public required Guid OrderId { get; init; }

    /// <summary>Stable identifier for this reservation row in Inventory DB.</summary>
    public required Guid ReservationId { get; init; }

    public required DateTimeOffset ReservedAt { get; init; }
}

/// <summary>Published by Inventory when stock reservation cannot be fulfilled.</summary>
public sealed class StockReservationFailed : InventoryIntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string Reason { get; init; }

    public required DateTimeOffset FailedAt { get; init; }
}

/// <summary>Published by the background expiry job when a reservation TTL elapses without confirmation.</summary>
public sealed class ReservationExpired : InventoryIntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required Guid ReservationId { get; init; }

    public required DateTimeOffset ExpiredAt { get; init; }
}
