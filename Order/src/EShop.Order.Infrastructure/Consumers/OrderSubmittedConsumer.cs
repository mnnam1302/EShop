using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Order.Infrastructure.Consumers;

public sealed class OrderSubmittedConsumer : IConsumer<OrderSubmitted>
{
    private readonly IMediator _mediator;

    public OrderSubmittedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        await _mediator.SendAsync(context.Message, context.CancellationToken);
    }
}
