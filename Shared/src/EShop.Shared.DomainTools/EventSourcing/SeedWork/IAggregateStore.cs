namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public interface IAggregateStore
{
    Task AppendEventsAsync(IAggregate aggregate, CancellationToken cancellationToken);

    Task<TAggregate> LoadAggregateAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken) where TAggregate : IAggregate, new();
}
