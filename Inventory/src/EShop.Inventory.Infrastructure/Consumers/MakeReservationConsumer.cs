using EShop.Inventory.Domain.Commands;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Inventory.Infrastructure.Consumers;

public sealed class MakeReservationConsumer(IMediator mediator) : IConsumer<MakeReservation>
{
    public async Task Consume(ConsumeContext<MakeReservation> context)
    {
        var message = context.Message;
        var command = new MakeReservationsCommand
        {
            OrderId = message.OrderId,
            Items = message.Items.Select(x => new Domain.Commands.OrderItem
            {
                VariantId = x.VariantId,
                Quantity = x.Quantity,
            }).ToList()
        };

        await mediator.SendAsync(command, context.CancellationToken);
    }
}