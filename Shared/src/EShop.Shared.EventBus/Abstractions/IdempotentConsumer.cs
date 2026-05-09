using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using MassTransit;

namespace EShop.Shared.EventBus.Abstractions;

[Obsolete("Each microservice is responsible for implementing idempotent consumers when needed. This reuse make code more complex and harder to maintain.")]
public abstract class IdempotentConsumer<T> : IConsumer<T>
    where T : class, IIntegrationEvent
{
    private readonly IMessageRepository messageRepository;

    protected IdempotentConsumer(IMessageRepository messageRepository)
    {
        this.messageRepository = messageRepository;
    }

    protected abstract Task<Result> HandleMessageAsync(T message, CancellationToken cancellationToken);

    public async Task Consume(ConsumeContext<T> context)
    {
        var messageId = context.MessageId!.Value;
        var message = context.Message;
        var consumerId = $"{GetType().Name}_{message.GetType().Name}";

        var alreadyProcessed = await messageRepository.ExistsAsync(messageId, consumerId, context.CancellationToken);
        if (alreadyProcessed)
        {
            return;
        }

        var result = await HandleMessageAsync(message, context.CancellationToken);
        var inboxMessage = InboxMessage.Create(consumerId, messageId, message.GetType().Name);

        if (result.IsSuccess)
        {
            inboxMessage.MarkAsCompleted();
        }
        else
        {
            inboxMessage.MarkAsFailed(result.Error.Message);
        }

        await messageRepository.AddAsync(inboxMessage, context.CancellationToken);
    }
}