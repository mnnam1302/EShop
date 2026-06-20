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
        var command = new ReserveStocksCommand
        {
            OrderId = message.OrderId,
            Items = message.Items.Select(x => new Domain.Commands.OrderItem
            {
                VariantId = x.VariantId,
                Quantity = x.Quantity,
            }).ToList(),
            TenantId = message.TenantId,
            ActionUserId = message.ActionUserId,
            ActionUserType = message.ActionUserType
        };

        await mediator.SendAsync(command, context.CancellationToken);
    }
}
