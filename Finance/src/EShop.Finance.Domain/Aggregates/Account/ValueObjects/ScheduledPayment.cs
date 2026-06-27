using EShop.Shared.DomainTools.ValueObjects;

namespace EShop.Finance.Domain.Aggregates.Account.ValueObjects;

public sealed class ScheduledPayment : ValueObject
{
    public ScheduledPayment()
    {
    }

    public ScheduledPayment(int sequence, decimal amount, DateOnly dueDate)
    {
        Sequence = sequence;
        Amount = amount;
        DueDate = dueDate;
    }

    public int Sequence { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Sequence;
        yield return Amount;
        yield return DueDate;
    }
}
