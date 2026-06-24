namespace EShop.Shared.Contracts.Services.Inventory;

/// <summary>
/// Sent by the saga to Inventory on payment success to move
/// a Pending reservation to Confirmed (no stock change).
/// </summary>
public sealed class ConfirmReservationCommand
{
    public required Guid OrderId { get; init; }
    public required Guid ReservationId { get; init; }
    public required string TenantId { get; init; }
    public required string ActionUserId { get; init; }
    public required string ActionUserType { get; init; }
}
