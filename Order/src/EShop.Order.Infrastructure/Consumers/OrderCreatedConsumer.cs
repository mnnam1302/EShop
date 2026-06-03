using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Order.Infrastructure.Consumers;

public sealed class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly IMediator _mediator;

    public OrderCreatedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        await _mediator.SendAsync(context.Message, context.CancellationToken);
    }
}
