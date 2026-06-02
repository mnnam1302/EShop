namespace EShop.Inventory.Domain.Enums;

public enum ReservationStatus
{
    /// <summary>Stock is held; order processing is in progress.</summary>
    Active,

    /// <summary>Reservation was released (order cancelled or compensated).</summary>
    Released,

    /// <summary>Reservation TTL expired without confirmation.</summary>
    Expired,

    /// <summary>Stock has been permanently deducted; reservation fulfilled.</summary>
    Confirmed
}
