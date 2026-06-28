using EShop.Finance.Domain.Enums;
using EShop.Finance.Domain.Services.PaymentSchedule;
using EShop.Shared.DomainTools.Exceptions;
using FluentAssertions;

namespace EShop.Finance.Tests;

public class PaymentScheduleCalculatorTests
{
    private static readonly DateOnly Start = new(2026, 1, 15);

    [Theory]
    [InlineData(PaymentFrequency.OneOff, 1)]
    [InlineData(PaymentFrequency.Annually, 1)]
    [InlineData(PaymentFrequency.Quarterly, 4)]
    [InlineData(PaymentFrequency.Monthly, 12)]
    public void Frequency_determines_instalment_count(string frequency, int expectedCount)
    {
        var schedule = PaymentScheduleCalculator.Calculate(120.00m, frequency, Start);

        schedule.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Unknown_frequency_is_rejected()
    {
        var act = () => PaymentScheduleCalculator.Calculate(120.00m, "Weekly", Start);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Even_division_splits_total_equally()
    {
        var schedule = PaymentScheduleCalculator.Calculate(120.00m, PaymentFrequency.Quarterly, Start);

        schedule.Select(i => i.Amount).Should().AllBeEquivalentTo(30.00m);
        schedule.Sum(i => i.Amount).Should().Be(120.00m);
    }

    [Fact]
    public void Instalment_amounts_always_sum_to_total_with_rounding()
    {
        // 100.00 split monthly (12) cannot divide evenly; amounts must still sum exactly.
        var schedule = PaymentScheduleCalculator.Calculate(100.00m, PaymentFrequency.Monthly, Start);

        schedule.Sum(i => i.Amount).Should().Be(100.00m);
        // First 11 share the rounded-down base; the last absorbs the remainder.
        schedule.Take(11).Select(i => i.Amount).Should().AllBeEquivalentTo(8.33m);
        schedule.Last().Amount.Should().Be(100.00m - (8.33m * 11));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Zero_or_negative_total_is_rejected(decimal total)
    {
        var act = () => PaymentScheduleCalculator.Calculate(total, PaymentFrequency.OneOff, Start);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Monthly_due_dates_advance_by_one_month()
    {
        var schedule = PaymentScheduleCalculator.Calculate(120.00m, PaymentFrequency.Monthly, Start);

        schedule[0].DueDate.Should().Be(new DateOnly(2026, 1, 15));
        schedule[1].DueDate.Should().Be(new DateOnly(2026, 2, 15));
        schedule[2].DueDate.Should().Be(new DateOnly(2026, 3, 15));
    }

    [Fact]
    public void Quarterly_due_dates_advance_by_three_months()
    {
        var schedule = PaymentScheduleCalculator.Calculate(120.00m, PaymentFrequency.Quarterly, Start);

        schedule.Select(i => i.DueDate).Should().Equal(
            new DateOnly(2026, 1, 15),
            new DateOnly(2026, 4, 15),
            new DateOnly(2026, 7, 15),
            new DateOnly(2026, 10, 15));
    }
}
