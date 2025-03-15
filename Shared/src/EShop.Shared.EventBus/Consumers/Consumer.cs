using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.EventBus.Consumers;

public abstract class Consumer<TMessage> : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    public virtual Task Consume(ConsumeContext<TMessage> context)
    {
        throw new NotImplementedException();
    }
}