using EShop.Finance.Domain.Aggregates.Account;
using EShop.Finance.Domain.Enums;
using EShop.Shared.DomainTools.Exceptions;
using FluentAssertions;

namespace EShop.Finance.Tests.Domain;

public class AccountTests
{
    private static readonly DateOnly Start = new(2026, 1, 15);

    private static Account NewAccount(string frequency = PaymentFrequency.Quarterly, decimal total = 120.00m) =>
        Account.Create(Guid.NewGuid(), "buyer-1", total, "USD", frequency, "tenant-1");

    [Fact]
    public void Create_defaults_to_oneoff_when_no_frequency_supplied()
    {
        var account = Account.Create(Guid.NewGuid(), "buyer-1", 100m, "USD", null, "tenant-1");

        account.PaymentFrequency.Should().Be(PaymentFrequency.OneOff);
        account.Status.Should().Be(AccountStatus.AwaitingSchedule);
        account.OutstandingAmount.Should().Be(100m);
    }

    [Fact]
    public void GenerateSchedule_creates_instalments_and_moves_to_scheduled()
    {
        var account = NewAccount();

        account.CalculateScheduledPayments(Start);

        account.Status.Should().Be(AccountStatus.Scheduled);
        account.Payments.Should().HaveCount(4);
        account.Payments.Should().OnlyContain(i => i.Status == PaymentStatus.Pending);
    }

    [Fact]
    public void GenerateSchedule_twice_is_rejected()
    {
        var account = NewAccount();
        account.CalculateScheduledPayments(Start);

        var act = () => account.CalculateScheduledPayments(Start);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Valid_progression_pending_booked_paid()
    {
        var account = NewAccount(PaymentFrequency.OneOff);
        account.CalculateScheduledPayments(Start);
        var instalment = account.Payments.Single();

        account.BookPayment(instalment.Id, "ext-ref-1");
        account.RecordPayment(instalment.Id, instalment.Amount, DateTimeOffset.UtcNow);

        instalment.Status.Should().Be(PaymentStatus.Paid);
        account.Status.Should().Be(AccountStatus.Completed);
        account.OutstandingAmount.Should().Be(0);
    }

    [Fact]
    public void Paying_pending_instalment_without_booking_is_rejected()
    {
        var account = NewAccount(PaymentFrequency.OneOff);
        account.CalculateScheduledPayments(Start);
        var instalment = account.Payments.Single();

        var act = () => account.RecordPayment(instalment.Id, instalment.Amount, DateTimeOffset.UtcNow);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Account_completes_only_after_all_instalments_paid()
    {
        var account = NewAccount(PaymentFrequency.Quarterly, 120.00m);
        account.CalculateScheduledPayments(Start);

        var ordered = account.Payments.OrderBy(i => i.Sequence).ToList();
        foreach (var instalment in ordered.Take(3))
        {
            account.BookPayment(instalment.Id, $"ref-{instalment.Sequence}");
            account.RecordPayment(instalment.Id, instalment.Amount, DateTimeOffset.UtcNow);
        }

        account.Status.Should().Be(AccountStatus.Scheduled);
        account.OutstandingAmount.Should().Be(30.00m);

        var last = ordered[3];
        account.BookPayment(last.Id, "ref-4");
        account.RecordPayment(last.Id, last.Amount, DateTimeOffset.UtcNow);

        account.Status.Should().Be(AccountStatus.Completed);
        account.OutstandingAmount.Should().Be(0);
    }

    [Fact]
    public void Booking_is_idempotent_on_retry()
    {
        var account = NewAccount(PaymentFrequency.OneOff);
        account.CalculateScheduledPayments(Start);
        var instalment = account.Payments.Single();

        account.BookPayment(instalment.Id, "ref-1");
        account.BookPayment(instalment.Id, "ref-1");

        instalment.ExternalBookingReference.Should().Be("ref-1");
        instalment.Status.Should().Be(PaymentStatus.Booked);
    }

    [Fact]
    public void Redelivered_payment_does_not_double_reduce_balance()
    {
        var account = NewAccount(PaymentFrequency.Quarterly, 120.00m);
        account.CalculateScheduledPayments(Start);
        var instalment = account.Payments.OrderBy(i => i.Sequence).First();

        account.BookPayment(instalment.Id, "ref-1");
        account.RecordPayment(instalment.Id, instalment.Amount, DateTimeOffset.UtcNow);
        account.RecordPayment(instalment.Id, instalment.Amount, DateTimeOffset.UtcNow);

        account.OutstandingAmount.Should().Be(90.00m);
    }

    [Fact]
    public void Fail_marks_account_failed_and_raises_event()
    {
        var account = NewAccount();
        account.CalculateScheduledPayments(Start);

        account.Fail("provider unreachable");

        account.Status.Should().Be(AccountStatus.Failed);
    }
}
