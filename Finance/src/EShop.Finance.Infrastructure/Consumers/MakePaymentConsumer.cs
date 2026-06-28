using EShop.Finance.Domain.Aggregates.Account.Commands;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Finance.Infrastructure.Consumers;

public sealed class MakePaymentConsumer(IMediator mediator) : IConsumer<MakePayment>
{
    public async Task Consume(ConsumeContext<MakePayment> context)
    {
        var message = context.Message;
        var command = new CreateAccountCommand
        {
            OrderId = message.OrderId,
            BuyerId = message.BuyerId,
            TotalAmount = message.TotalAmount,
            Currency = message.Currency,
            PaymentFrequency = message.PaymentFrequency,
            TenantId = message.TenantId,
            ActionUserId = message.ActionUserId,
            ActionUserType = message.ActionUserType,
        };

        await mediator.SendAsync(command, context.CancellationToken);
    }
}
