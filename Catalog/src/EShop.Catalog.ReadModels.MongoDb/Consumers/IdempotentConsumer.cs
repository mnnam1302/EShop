using EShop.Catalog.ReadModels.MongoDb.Infrastructure;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using MassTransit;

namespace EShop.Catalog.ReadModels.MongoDb.Consumers;

public abstract class IdempotentConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : CatalogIntegrationEvent
{
    private readonly IMongoRepositoryBase<InboxMessage> _mongoRepository;

    protected IdempotentConsumer(IMongoRepositoryBase<InboxMessage> mongoRepository)
    {
        _mongoRepository = mongoRepository;
    }

    protected abstract Task<Result> HandleMessageAsync(TMessage message, CancellationToken cancellationToken);

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var message = context.Message;
        var consumerId = $"{GetType().Name}_{message.GetType().Name}";
        var messageId = message.EventId;

        var alreadyProcessed = await _mongoRepository.FindOneAsync(
            inboxMessage => inboxMessage.DocumentId == messageId && inboxMessage.ConsumerId == consumerId,
            context.CancellationToken);

        if (alreadyProcessed is not null)
        {
            return;
        }

        var inboxMessage = InboxMessage.Create(consumerId, messageId, message.GetType().Name);

        var result = await HandleMessageAsync(message, context.CancellationToken);

        if (result.IsSuccess)
        {
            inboxMessage.MarkAsDone();
        }
        else
        {
            inboxMessage.MarkAsFailed(result.Error.Message);
        }

        await _mongoRepository.InsertOneAsync(inboxMessage, context.CancellationToken);
    }
}