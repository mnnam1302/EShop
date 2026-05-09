using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.EventBus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EShop.Catalog.Application.Shared;

public abstract class Consumer<TMessage> : IConsumer<TMessage> 
    where TMessage : IntegrationEvent
{
    private readonly CatalogDbContext _dbContext;

    protected Consumer(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        var message = context.Message;
        var messageId = context.MessageId ?? message.EventId;
        var consumerId = $"{GetType().Name}_{message.GetType().Name}";

        var existingMessage = await _dbContext.InboxMessages.AnyAsync(m =>
            m.MessageId == messageId && m.ConsumerId == consumerId, context.CancellationToken);

        if (existingMessage)
        {
            return;
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(context.CancellationToken);

        try
        {
            var inboxMessage = InboxMessage.Create(consumerId, messageId, typeof(TMessage).Name);

            _dbContext.InboxMessages.Add(inboxMessage);
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            var result = await HandleMessageAsync(context.Message, context.CancellationToken);

            if (result.IsSuccess)
            {
                inboxMessage.MarkAsCompleted();
            }
            else
            {
                // Handling the message failed depend on business requirements
                inboxMessage.MarkAsFailed(result.Error.Message);
            }

            await _dbContext.SaveChangesAsync(context.CancellationToken);

            await transaction.CommitAsync(context.CancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Duplicate message contraint violation, another consumer has processed the same message concurrently
            await transaction.RollbackAsync(context.CancellationToken);

            return;
        }
    }

    protected abstract Task<Result> HandleMessageAsync(TMessage message, CancellationToken cancellationToken);
}
