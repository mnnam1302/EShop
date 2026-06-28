using System.ComponentModel.DataAnnotations;
using EShop.Inventory.Domain.Commands;
using EShop.Inventory.Domain.DomainEvents;
using EShop.Inventory.Domain.Enums;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using Microsoft.Extensions.Logging;

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

    private readonly List<ReservationItem> _items = new();
    public virtual IReadOnlyCollection<ReservationItem> Items => _items;

    public static Reservation Create(MakeReservationCommand command, DateTimeOffset expiresAt)
    {
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            OrderId = command.OrderId,
            Status = nameof(ReservationStatus.Pending),
            ExpiresAt = expiresAt,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            TenantId = command.TenantId,
            Scope = command.TenantId
        };

        reservation.RaiseDomainEvent(new ReservationCreated
        {
            ReservationId = reservation.Id,
            OrderId = command.OrderId,
            TenantId = command.TenantId,
            ActionUserId = command.ActionUserId,
            ActionUserType = command.ActionUserType
        });

        return reservation;
    }

    public ReservationItem AddItem(Guid variantId, int quantity)
    {
        var item = new ReservationItem
        {
            Id = Guid.NewGuid(),
            ReservationId = Id,
            VariantId = variantId,
            Quantity = quantity,
            TenantId = TenantId,
            Scope = TenantId
        };

        _items.Add(item);
        return item;
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
