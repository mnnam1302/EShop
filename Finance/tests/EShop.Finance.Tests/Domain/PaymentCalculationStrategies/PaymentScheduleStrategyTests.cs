using EShop.Finance.Domain.Enums;
using EShop.Finance.Domain.Services.PaymentSchedule;
using EShop.Shared.DomainTools.Exceptions;
using FluentAssertions;

namespace EShop.Finance.Tests.Domain.PaymentCalculationStrategies;

public class PaymentScheduleStrategyTests
{
    [Theory]
    [InlineData(PaymentFrequency.OneOff, typeof(OneOffPaymentScheduleStrategy))]
    [InlineData(PaymentFrequency.Monthly, typeof(MonthlyPaymentScheduleStrategy))]
    [InlineData(PaymentFrequency.Quarterly, typeof(QuarterlyPaymentScheduleStrategy))]
    [InlineData(PaymentFrequency.Annually, typeof(AnnualPaymentScheduleStrategy))]
    public void Factory_resolves_the_strategy_for_each_frequency(string frequency, Type expected)
    {
        var strategy = PaymentScheduleStrategyFactory.Resolve(frequency);

        strategy.Should().BeOfType(expected);
        strategy.Frequency.Should().Be(frequency);
    }

    [Fact]
    public void Factory_throws_for_an_unknown_frequency()
    {
        var act = () => PaymentScheduleStrategyFactory.Resolve("Weekly");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_strategy_builds_its_own_schedule_independently()
    {
        var strategy = new QuarterlyPaymentScheduleStrategy();

        var schedule = strategy.Calculate(120.00m, new DateOnly(2026, 1, 15), minorUnitDigits: 2);

        schedule.Should().HaveCount(4);
        schedule.Sum(i => i.Amount).Should().Be(120.00m);
    }
}
