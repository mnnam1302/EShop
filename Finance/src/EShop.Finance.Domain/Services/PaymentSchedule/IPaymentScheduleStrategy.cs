using EShop.Finance.Domain.Aggregates.Account.ValueObjects;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>
/// Strategy that knows how to split an order total into dated instalments for ONE payment
/// frequency. Each frequency is a separate strategy, so adding a new frequency means adding a
/// strategy — no edits to existing ones (Open/Closed). Strategies are pure, stateless domain
/// services: no infrastructure, no DI.
/// </summary>
public interface IPaymentScheduleStrategy
{
    /// <summary>The <c>PaymentFrequency</c> value this strategy handles.</summary>
    string Frequency { get; }

    /// <summary>Builds the instalment schedule for <paramref name="total"/> starting on <paramref name="startDate"/>.</summary>
    IReadOnlyList<ScheduledPayment> Calculate(decimal total, DateOnly startDate, int minorUnitDigits);
}
