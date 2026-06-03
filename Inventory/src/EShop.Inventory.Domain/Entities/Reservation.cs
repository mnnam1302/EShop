using EShop.Inventory.Domain.Enums;
using EShop.Shared.DomainTools.Aggregates;

namespace EShop.Inventory.Domain.Entities;

public class Reservation : AggregateRoot<Guid>
{
    public required Guid OrderId { get; init; }

    public required Guid VariantId { get; init; }

    public required int Quantity { get; init; }

    public ReservationStatus Status { get; private set; } = ReservationStatus.Active;

    public required DateTimeOffset ExpiresAt { get; init; }

    public required DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? ReleasedAtUtc { get; private set; }

    public static Reservation Create(
        Guid orderId,
        Guid variantId,
        int quantity,
        DateTimeOffset expiresAt)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            VariantId = variantId,
            Quantity = quantity,
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
