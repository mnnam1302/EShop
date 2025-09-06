using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.EventBus.Consumers;

public abstract class Consumer<TMessage, TDbContext> : IConsumer<TMessage>
    where TMessage : class, IMessage
    where TDbContext : DbContext, IInboxDbContext
{
    private readonly TDbContext _dbContext;

    protected Consumer(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        await HandleInboxMessage(context, context.CancellationToken);
    }

    protected abstract Task<Result> HandleMessageAsync(TMessage message, CancellationToken cancellationToken);

    private async Task HandleInboxMessage(ConsumeContext<TMessage> context, CancellationToken cancellationToken)
    {
        var message = context.Message;
        var messageId = context.Headers.Get<Guid>("OutboxMessageId") ?? context.MessageId;

        var existingInbox = await _dbContext.InboxMessages
            .AnyAsync(x => x.Id == messageId, cancellationToken);

        if (!existingInbox)
        {
            var result = await HandleMessageAsync(message, cancellationToken);

            var consumerId = $"{GetType().Name}:{message.GetType().Name}";
            var inboxMessage = new InboxMessage
            {
                Id = messageId!.Value,
                MessageType = message.GetType().Name,
                ConsumerId = consumerId,
                State = InboxMessageStatus.New,
                CreatedOnUtc = DateTime.UtcNow
            };

            if (result.IsSuccess)
            {
                inboxMessage.State = InboxMessageStatus.Done;
            }
            else
            {
                inboxMessage.State = InboxMessageStatus.Failed;
                inboxMessage.ReasonFailed = result.Error.Message;
            }

            inboxMessage.UpdatedOnUtc = DateTime.UtcNow;

            _dbContext.InboxMessages.Add(inboxMessage);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}