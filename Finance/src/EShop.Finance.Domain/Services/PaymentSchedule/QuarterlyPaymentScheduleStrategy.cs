using EShop.Finance.Domain.Enums;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>Four instalments over a one-year term, three months apart.</summary>
public sealed class QuarterlyPaymentScheduleStrategy : PaymentScheduleStrategy
{
    public override string Frequency => PaymentFrequency.Quarterly;

    protected override int PaymentCount => 4;

    protected override DateOnly NextDueDate(DateOnly previous) => previous.AddMonths(3);
}
