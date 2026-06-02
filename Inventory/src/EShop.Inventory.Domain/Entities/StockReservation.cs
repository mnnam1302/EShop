using EShop.Inventory.Domain.Enums;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Inventory.Domain.Entities;

/// <summary>
/// Represents a stock hold created during order placement.
/// One row per order per attempt; the idempotency key prevents duplicate roớws.
/// </summary>
public sealed class StockReservation : EntityBase<Guid>
{
    public required Guid OrderId { get; init; }

    public required Guid VariantId { get; init; }

    public required int Quantity { get; init; }

    /// <summary>
    /// Deterministic key used for deduplication.
    /// Sourced from the saga's CorrelationId (= OrderId) so retry-safe.
    /// </summary>
    public required Guid IdempotencyKey { get; init; }

    public ReservationStatus Status { get; private set; } = ReservationStatus.Active;

    public required DateTimeOffset ExpiresAt { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? ReleasedAtUtc { get; private set; }

    public static StockReservation Create(
        Guid orderId,
        Guid variantId,
        int quantity,
        Guid idempotencyKey,
        DateTimeOffset expiresAt)
    {
        return new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            VariantId = variantId,
            Quantity = quantity,
            IdempotencyKey = idempotencyKey,
            ExpiresAt = expiresAt,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void Release()
    {
        Status = ReservationStatus.Released;
        ReleasedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Expire()
    {
        Status = ReservationStatus.Expired;
        ReleasedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Confirm()
    {
        Status = ReservationStatus.Confirmed;
    }
}
