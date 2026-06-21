namespace EShop.Inventory.Domain.Abstractions;

public interface IOutboxWriter
{
    void Enqueue<TEvent>(TEvent @event) where TEvent : class;
}
