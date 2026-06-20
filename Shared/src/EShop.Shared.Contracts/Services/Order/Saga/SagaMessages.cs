namespace EShop.Shared.Contracts.Services.Order.Saga;

/// <summary>
/// Sent by the saga to the Inventory service to release a previously created
/// reservation (compensation path).
/// </summary>
public sealed class ReleaseReservationCommand
{
    public required Guid OrderId { get; init; }

    public required Guid ReservationId { get; init; }

    public required string TenantId { get; init; }

    public required string ActionUserId { get; init; }

    public required string ActionUserType { get; init; }
}

/// <summary>
/// Sent by the saga to the Order write-side (its own service) once stock is
/// reserved, to persist the canonical order row and order items.
/// </summary>
public sealed class PersistOrderCommand
{
    public required Guid OrderId { get; init; }

    public required string BuyerId { get; init; }

    public required Guid ReservationId { get; init; }

    public required IReadOnlyList<OrderItem> Items { get; init; }

    public required string TenantId { get; init; }

    public required string ActionUserId { get; init; }

    public required string ActionUserType { get; init; }
}

/// <summary>
/// Published by the Order write-side consumer after successfully persisting
/// the order row. Consumed by the saga to advance to the final state.
/// </summary>
public sealed class OrderPersisted
{
    public required Guid OrderId { get; init; }

    public required DateTimeOffset PersistedAt { get; init; }
}
