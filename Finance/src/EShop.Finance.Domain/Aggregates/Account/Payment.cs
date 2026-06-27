using System.ComponentModel.DataAnnotations;
using EShop.Finance.Domain.Enums;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Finance.Domain.Aggregates.Account;

/// <summary>
/// One scheduled payment within an <see cref="Account"/>. Moves through
/// Pending → Booked → Paid, with Booked → Failed permitted on booking/payment failure.
/// Illegal transitions throw a <see cref="DomainException"/>.
/// </summary>
public class Payment : EntityBase<Guid>, IScoped
{
    private const string ErrorTitle = "Payment";

    public Guid AccountId { get; set; }

    public int Sequence { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public required string Currency { get; set; }

    public DateOnly DueDate { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(ModelConstants.LongText)]
    public string? ExternalBookingReference { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateTimeOffset? PaidAtUtc { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public string? FailureReason { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public required string TenantId { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public required string Scope { get; set; }

    public bool IsSettled => Status == PaymentStatus.Paid;

    /// <summary>
    /// Marks the payment booked. Idempotent for retries: re-booking an already-booked
    /// payment keeps the existing reference and does not transition again.
    /// </summary>
    public void MarkBooked(string externalReference)
    {
        if (Status == PaymentStatus.Booked)
        {
            return;
        }

        if (Status != PaymentStatus.Pending)
        {
            throw new DomainException(ErrorTitle, $"Cannot book payment {Sequence} from state {Status}.");
        }

        Status = PaymentStatus.Booked;
        ExternalBookingReference = externalReference;
        FailureReason = null;
    }

    public void MarkPaid(decimal amount, DateTimeOffset paidAtUtc)
    {
        if (Status == PaymentStatus.Paid)
        {
            return;
        }

        if (Status != PaymentStatus.Booked)
        {
            throw new DomainException(ErrorTitle, $"Cannot pay payment {Sequence} from state {Status}; it must be booked first.");
        }

        Status = PaymentStatus.Paid;
        PaidAmount = amount;
        PaidAtUtc = paidAtUtc;
    }

    public void MarkFailed(string reason)
    {
        if (Status == PaymentStatus.Paid)
        {
            throw new DomainException(ErrorTitle, $"Cannot fail payment {Sequence}; it is already paid.");
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }
}
