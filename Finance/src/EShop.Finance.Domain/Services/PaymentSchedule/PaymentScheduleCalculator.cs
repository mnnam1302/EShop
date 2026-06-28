using EShop.Finance.Domain.Aggregates.Account.ValueObjects;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>
/// Domain service that turns an order total + <c>PaymentFrequency</c> into a dated instalment
/// schedule. It validates the total and delegates the per-frequency calculation to the
/// <see cref="IPaymentScheduleStrategy"/> chosen by <see cref="PaymentScheduleStrategyFactory"/>.
/// Pure and deterministic — no infrastructure — so it is exhaustively unit-testable.
/// </summary>
public static class PaymentScheduleCalculator
{
    private const string ErrorTitle = "PaymentSchedule";

    public static IReadOnlyList<ScheduledPayment> Calculate(
        decimal total,
        string paymentFrequency,
        DateOnly startDate,
        int minorUnitDigits = 2)
    {
        if (total <= 0)
        {
            throw new DomainException(ErrorTitle, "Order total must be greater than zero to build a payment schedule.");
        }

        var strategy = PaymentScheduleStrategyFactory.Resolve(paymentFrequency);
        return strategy.Calculate(total, startDate, minorUnitDigits);
    }
}
