namespace EShop.Shared.DomainTools.Sagas.AggregateSagas;

public interface IAggregateSagaStore
{
    Task<TSagaAggregate> LoadAggregateSagaAsync<TSagaAggregate>(
        Guid aggregateSagaId,
        CancellationToken cancellationToken) where TSagaAggregate : IAggregateSaga, new();

    Task UpdateAggregateSagaAsync(IAggregateSaga aggregateSaga, CancellationToken cancellationToken);
}
