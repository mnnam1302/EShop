using EShop.Finance.Domain.Enums;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>Twelve instalments over a one-year term, one month apart.</summary>
public sealed class MonthlyPaymentScheduleStrategy : PaymentScheduleStrategy
{
    public override string Frequency => PaymentFrequency.Monthly;

    protected override int PaymentCount => 12;

    protected override DateOnly NextDueDate(DateOnly previous) => previous.AddMonths(1);
}
