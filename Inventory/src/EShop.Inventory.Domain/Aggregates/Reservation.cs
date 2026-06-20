using System.ComponentModel.DataAnnotations;
using EShop.Inventory.Domain.Enums;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;

namespace EShop.Inventory.Domain.Aggregates;

public class Reservation : AggregateRoot<Guid>, IScoped, IDateTracking
{
    public required Guid OrderId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string Status { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ReleasedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string TenantId { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public required string Scope { get; set; }

    public static Reservation Create(Guid orderId, DateTimeOffset expiresAt, string tenantId)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = nameof(ReservationStatus.Pending),
            ExpiresAt = expiresAt,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            TenantId = tenantId,
            Scope = tenantId
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
