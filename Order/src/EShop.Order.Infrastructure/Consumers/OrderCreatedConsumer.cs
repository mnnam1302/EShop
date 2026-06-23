using EShop.Order.Domain.Sagas;
using EShop.Order.Infrastructure.BackgroundJobs;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS;
using Hangfire;
using MassTransit;

namespace EShop.Order.Infrastructure.Consumers;

public sealed class OrderCreatedConsumer(
    IMediator mediator,
    IBackgroundJobClient backgroundJobClient) : IConsumer<OrderCreated>
{
    private static readonly TimeSpan SagaTimeout = TimeSpan.FromMinutes(15);

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        await mediator.SendAsync(context.Message, context.CancellationToken);

        var sagaId = OrderSagaId.FromOrderId(context.Message.OrderId);

        backgroundJobClient.Schedule<OrderSagaExpireJob>(
            job => job.Execute(sagaId, context.Message.OrderId, context.CancellationToken),
            SagaTimeout);
    }
}
