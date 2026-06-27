using EShop.Finance.Domain.Enums;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>Pay the whole total in a single instalment, due on the start date.</summary>
public sealed class OneOffPaymentScheduleStrategy : PaymentScheduleStrategy
{
    public override string Frequency => PaymentFrequency.OneOff;

    protected override int PaymentCount => 1;

    protected override DateOnly NextDueDate(DateOnly previous) => previous;
}
