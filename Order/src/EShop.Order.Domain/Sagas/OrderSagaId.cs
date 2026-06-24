using EventFlow.Core;

namespace EShop.Order.Domain.Sagas;

public sealed class OrderSagaId : Identity<OrderSagaId>
{
    private static readonly Guid Namespace = Guid.Parse("769077C6-F84D-46E3-AD2E-828A576AAAF3");

    public OrderSagaId(string value) : base(value)
    {
    }

    public static Guid FromOrderId(Guid orderId)
    {
        return NewDeterministic(Namespace, orderId.ToString()).GetGuid();
    }
}
