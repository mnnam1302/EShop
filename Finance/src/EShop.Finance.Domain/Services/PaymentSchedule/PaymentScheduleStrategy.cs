using EShop.Finance.Domain.Aggregates.Account.ValueObjects;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>
/// Base strategy holding the shared distribution rule: split the total evenly at the currency
/// minor unit and let the FINAL payment absorb any rounding remainder, so payment amounts
/// always sum to the total exactly. Subclasses supply only the payment count and the interval.
/// </summary>
public abstract class PaymentScheduleStrategy : IPaymentScheduleStrategy
{
    public abstract string Frequency { get; }

    /// <summary>Number of payments this frequency produces over the term.</summary>
    protected abstract int PaymentCount { get; }

    /// <summary>Advances a due date to the next payment's due date.</summary>
    protected abstract DateOnly NextDueDate(DateOnly previous);

    public IReadOnlyList<ScheduledPayment> Calculate(decimal total, DateOnly startDate, int minorUnitDigits)
    {
        var count = PaymentCount;
        var step = (decimal)Math.Pow(10, -minorUnitDigits);

        // Round the even share DOWN to the minor unit so the remainder is non-negative
        // and lands on the final payment.
        var baseAmount = Math.Floor(total / count / step) * step;

        var payments = new List<ScheduledPayment>(count);
        var allocated = 0m;
        var dueDate = startDate;

        for (var i = 0; i < count; i++)
        {
            var isLast = i == count - 1;
            var amount = isLast ? total - allocated : baseAmount;
            allocated += amount;

            payments.Add(new ScheduledPayment(i + 1, amount, dueDate));
            dueDate = NextDueDate(dueDate);
        }

        return payments;
    }
}
