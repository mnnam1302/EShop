using EShop.Finance.Application.UseCases.BookInstalments;
using EShop.Finance.Domain.Aggregates.Account.Commands;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.CQRS;
using MassTransit;

namespace EShop.Finance.Infrastructure.Consumers;

/// <summary>
/// Consumes the <see cref="MakePayment"/> saga command from the Order process manager: opens a
/// finance account for the order, then books its instalments. Idempotent —
/// <see cref="CreateAccountCommand"/> no-ops if the account already exists, and booking carries a
/// deterministic idempotency key.
/// </summary>
public sealed class MakePaymentConsumer(IMediator mediator) : IConsumer<MakePayment>
{
    public async Task Consume(ConsumeContext<MakePayment> context)
    {
        var message = context.Message;

        var createResult = await mediator.SendAsync(new CreateAccountCommand
        {
            OrderId = message.OrderId,
            BuyerId = message.BuyerId,
            TotalAmount = message.TotalAmount,
            Currency = message.Currency,
            PaymentFrequency = message.PaymentFrequency,
            TenantId = message.TenantId,
        }, context.CancellationToken);

        if (createResult.IsFailure)
        {
            throw new InvalidOperationException(createResult.Error.Message);
        }

        var bookResult = await mediator.SendAsync(new BookInstalmentsCommand
        {
            OrderId = message.OrderId,
            TenantId = message.TenantId,
        }, context.CancellationToken);

        if (bookResult.IsFailure)
        {
            throw new InvalidOperationException(bookResult.Error.Message);
        }
    }
}
