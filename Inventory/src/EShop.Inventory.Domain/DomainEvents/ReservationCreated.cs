namespace EShop.Inventory.Domain.DomainEvents;

public sealed class ReservationCreated : ReservationDomainEvent
{
    public required Guid OrderId { get; init; }
    public required Guid ReservationId { get; init; }
    public required string TenantId { get; init; }
    public required string ActionUserId { get; init; }
    public required string ActionUserType { get; init; }
}
