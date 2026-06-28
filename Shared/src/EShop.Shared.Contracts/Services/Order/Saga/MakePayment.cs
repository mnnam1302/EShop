using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order.Saga;

/// <summary>
/// Saga command issued by the Order process manager once inventory is reserved and the order is
/// ready to be paid for. Finance consumes it to open an account and build the payment schedule.
/// Mirrors <see cref="MakeReservation"/> — an asynchronous command to a downstream service.
/// </summary>
public sealed class MakePayment : IntegrationCommand
{
    public required Guid OrderId { get; init; }
    public required string BuyerId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string Currency { get; init; }

    /// <summary>
    /// OneOff / Monthly / Quarterly / Annually. When null, Finance defaults to OneOff.
    /// </summary>
    public string? PaymentFrequency { get; init; }
}

public static class PaymentFrequencyConstants
{
    public const string OneOff = "OneOff";
    public const string Monthly = "Monthly";
    public const string Quarterly = "Quarterly";
    public const string Annually = "Annually";
}
