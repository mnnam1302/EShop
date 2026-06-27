using EShop.Finance.Domain.Enums;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>A single instalment for the year, due on the start date.</summary>
public sealed class AnnualPaymentScheduleStrategy : PaymentScheduleStrategy
{
    public override string Frequency => PaymentFrequency.Annually;

    protected override int PaymentCount => 1;

    protected override DateOnly NextDueDate(DateOnly previous) => previous.AddYears(1);
}
