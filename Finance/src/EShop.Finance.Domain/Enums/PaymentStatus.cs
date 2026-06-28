namespace EShop.Finance.Domain.Enums;

public enum PaymentStatus
{
    /// <summary>Scheduled but not yet booked with the accounting provider.</summary>
    Pending,

    /// <summary>Booked with the accounting provider; awaiting payment.</summary>
    Booked,

    /// <summary>Paid in full.</summary>
    Paid,

    /// <summary>Booking or payment failed.</summary>
    Failed,
}
