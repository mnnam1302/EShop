using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Exceptions;
using Microsoft.Extensions.Options;

namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public interface IEventStoreGateway
{
    Task AppendEventsAsync(IAggregate aggregate, CancellationToken cancellationToken);

    Task<TAggregate> LoadAggregateAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken) where TAggregate : IAggregate, new();
}