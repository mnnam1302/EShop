using System.ComponentModel.DataAnnotations;
using EShop.Inventory.Domain.Enums;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Inventory.Domain.Aggregates;

public class Reservation : AggregateRoot<Guid>, IDateTracking
{
    public required Guid OrderId { get; set; }
    public required Guid VariantId { get; set; }
    public required int Quantity { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string Status { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ReleasedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public static Reservation Create(Guid orderId, Guid variantId, int quantity, DateTimeOffset expiresAt)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            VariantId = variantId,
            Quantity = quantity,
            Status = nameof(ReservationStatus.Active),
            ExpiresAt = expiresAt,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public void Confirm()
    {
        Status = nameof(ReservationStatus.Confirmed);
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Release()
    {
        Status = nameof(ReservationStatus.Released);
        ReleasedAtUtc = DateTimeOffset.UtcNow;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Expire()
    {
        Status = nameof(ReservationStatus.Expired);
        ReleasedAtUtc = DateTimeOffset.UtcNow;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }
}
