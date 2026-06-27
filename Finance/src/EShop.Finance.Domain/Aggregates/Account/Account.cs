using System.ComponentModel.DataAnnotations;
using EShop.Finance.Domain.Aggregates.Account.DomainEvents;
using EShop.Finance.Domain.Enums;
using EShop.Finance.Domain.Services.PaymentSchedule;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Finance.Domain.Aggregates.Account;

/// <summary>
/// Owns the payment lifecycle of a single order: total, chosen <see cref="PaymentFrequency"/>,
/// the generated payments, and outstanding balance.
/// </summary>
public class Account : AggregateRoot<Guid>, IScoped, IDateTracking
{
    private const string ErrorTitle = "Account";

    public Guid OrderId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string BuyerId { get; set; }

    public decimal TotalAmount { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public required string Currency { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string PaymentFrequency { get; set; }

    public AccountStatus Status { get; set; } = AccountStatus.AwaitingSchedule;

    public decimal OutstandingAmount { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string TenantId { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public required string Scope { get; set; }

    private readonly List<Payment> _payments = new();
    public virtual IReadOnlyCollection<Payment> Payments => _payments;

    public static Account Create(
        Guid orderId,
        string buyerId,
        decimal totalAmount,
        string currency,
        string? paymentFrequency,
        string tenantId)
    {
        var frequency = string.IsNullOrWhiteSpace(paymentFrequency)
            ? Enums.PaymentFrequency.OneOff
            : paymentFrequency;

        var account = new Account
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            BuyerId = buyerId,
            TotalAmount = totalAmount,
            Currency = currency,
            PaymentFrequency = frequency,
            Status = AccountStatus.AwaitingSchedule,
            OutstandingAmount = totalAmount,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            TenantId = tenantId,
            Scope = tenantId,
        };

        account.RaiseDomainEvent(new AccountCreated
        {
            AccountId = account.Id,
            OrderId = orderId,
            TenantId = tenantId,
        });

        return account;
    }

    /// <summary>
    /// Generates the payment schedule from the total and frequency. May only be called once,
    /// while the account is still <see cref="AccountStatus.AwaitingSchedule"/>.
    /// </summary>
    public void CalculateScheduledPayments(DateOnly startDate)
    {
        if (Status != AccountStatus.AwaitingSchedule)
        {
            throw new DomainException(ErrorTitle, $"Schedule already generated for account {Id}.");
        }

        var scheduled = PaymentScheduleCalculator.Calculate(TotalAmount, PaymentFrequency, startDate);

        foreach (var item in scheduled)
        {
            _payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                AccountId = Id,
                Sequence = item.Sequence,
                Amount = item.Amount,
                Currency = Currency,
                DueDate = item.DueDate,
                Status = PaymentStatus.Pending,
                TenantId = TenantId,
                Scope = TenantId,
            });
        }

        AssertScheduleIntegrity();

        Status = AccountStatus.Scheduled;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PaymentScheduled
        {
            AccountId = Id,
            OrderId = OrderId,
            PaymentCount = _payments.Count,
        });
    }

    /// <summary>
    /// Invariant: the generated payments MUST sum to the account total. The aggregate enforces
    /// this itself rather than trusting the calculator (mirrors the reference service's
    /// schedule-integrity assertion).
    /// </summary>
    private void AssertScheduleIntegrity()
    {
        var totalScheduled = _payments.Sum(i => i.Amount);
        if (totalScheduled != TotalAmount)
        {
            throw new DomainException(ErrorTitle, $"Schedule integrity violated: payments sum to {totalScheduled} but account total is {TotalAmount}.");
        }
    }

    public void BookPayment(Guid paymentId, string externalReference)
    {
        var payment = RequirePayment(paymentId);
        payment.MarkBooked(externalReference);
        LastModifiedAtUtc = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new PaymentBooked
        {
            AccountId = Id,
            PaymentId = paymentId,
            ExternalReference = externalReference,
        });
    }

    public void RecordPayment(Guid paymentId, decimal amount, DateTimeOffset paidAtUtc)
    {
        var payment = RequirePayment(paymentId);
        var wasSettled = payment.IsSettled;
        payment.MarkPaid(amount, paidAtUtc);
        LastModifiedAtUtc = DateTimeOffset.UtcNow;

        if (!wasSettled)
        {
            OutstandingAmount -= payment.Amount;

            RaiseDomainEvent(new PaymentPaid
            {
                AccountId = Id,
                PaymentId = paymentId,
                Amount = payment.Amount,
            });
        }

        if (Status != AccountStatus.Completed && _payments.All(i => i.IsSettled))
        {
            Status = AccountStatus.Completed;
            OutstandingAmount = 0;

            RaiseDomainEvent(new AccountCompleted
            {
                AccountId = Id,
                OrderId = OrderId,
            });
        }
    }

    public void Fail(string reason)
    {
        if (Status == AccountStatus.Completed)
        {
            throw new DomainException(ErrorTitle, $"Cannot fail account {Id}; it is already completed.");
        }

        Status = AccountStatus.Failed;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new AccountFailed
        {
            AccountId = Id,
            OrderId = OrderId,
            Reason = reason,
        });
    }

    private Payment RequirePayment(Guid paymentId)
    {
        return _payments.FirstOrDefault(i => i.Id == paymentId)
            ?? throw new DomainException(ErrorTitle, $"Payment {paymentId} not found on account {Id}.");
    }
}
