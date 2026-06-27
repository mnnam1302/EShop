namespace EShop.Finance.Domain.Enums;

public enum AccountStatus
{
    /// <summary>Account created from an order event; no schedule generated yet.</summary>
    AwaitingSchedule,

    /// <summary>Instalments generated; awaiting booking and payment.</summary>
    Scheduled,

    /// <summary>Every instalment is paid.</summary>
    Completed,

    /// <summary>Payment failed terminally; the order should be released.</summary>
    Failed,
}
