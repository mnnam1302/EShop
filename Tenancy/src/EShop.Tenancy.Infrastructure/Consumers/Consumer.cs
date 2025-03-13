using MassTransit;
using MediatR;

namespace EShop.Tenancy.Infrastructure.Consumers;

public abstract class Consumer<TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    private readonly ISender _sender;

    protected Consumer(ISender sender)
    {
        _sender = sender;
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        // Handle inbox message

        await _sender.Send(context.Message);
    }
}