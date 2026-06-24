using EShop.Shared.DomainTools.Aggregates;

namespace EShop.Inventory.Domain.Abstractions;

public interface IOutboxWriter
{
    void ConvertDomainEventsToOutboxMessages<TAggregate>(string aggregateId, TAggregate aggregate) where TAggregate : IAggregateRoot;
}
