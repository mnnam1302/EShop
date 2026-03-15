using EShop.Catalog.ReadModels.MongoDb.Persistence;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.EventBus;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EShop.Catalog.ReadModels.MongoDb.Consumers;

public abstract class IdempotentConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : IntegrationEvent
{
    private readonly CatalogReadDbContext _dbContext;

    protected IdempotentConsumer(CatalogReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected abstract Task<Result> HandleMessageAsync(TMessage message, CancellationToken cancellationToken);

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var message = context.Message;
        var consumerId = $"{GetType().Name}_{message.GetType().Name}";
        var messageId = message.EventId;

        var alreadyProcessed = await _dbContext.InboxMessages
            .IgnoreQueryFilters()
            .AnyAsync(
                m => m.MessageId == messageId && m.ConsumerId == consumerId,
                context.CancellationToken);

        if (alreadyProcessed)
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

        _dbContext.InboxMessages.Add(inboxMessage);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}