using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using MassTransit;

namespace EShop.Shared.EventBus.Abstractions;

public abstract class IdempotentConsumer<T>(IMessageRepository messageRepository) : IConsumer<T>
    where T : class, IIntegrationEvent
{
    protected abstract Task<Result> HandleMessageAsync(T message, CancellationToken cancellationToken);

    public async Task Consume(ConsumeContext<T> context)
    {
        var message = context.Message;
        var messageId = message.EventId;
        var consumerId = $"{GetType().Name}_{message.GetType().Name}";

        var alreadyProcessed = await messageRepository.ExistsAsync(messageId, consumerId, context.CancellationToken);
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

        await messageRepository.AddAsync(inboxMessage, context.CancellationToken);
    }
}