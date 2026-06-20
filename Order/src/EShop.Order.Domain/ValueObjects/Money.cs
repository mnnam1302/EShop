using EShop.Shared.DomainTools.ValueObjects;

namespace EShop.Order.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string CurrencyCode { get; private set; }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return CurrencyCode;
    }
}
