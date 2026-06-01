using EShop.Order.Domain.Commands;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Order.Saga;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Infrastructure.Consumers;

internal sealed class PersistOrderConsumer : IConsumer<PersistOrderCommand>
{
    private readonly OrderDbContext _dbContext;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<PersistOrderConsumer> _logger;

    public PersistOrderConsumer(
        OrderDbContext dbContext,
        IUserDetailsProvider userDetailsProvider,
        ILogger<PersistOrderConsumer> logger)
    {
        _dbContext = dbContext;
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PersistOrderCommand> context)
    {
        var cmd = context.Message;

        using var _ = _userDetailsProvider.CreateSystemUserScope(cmd.TenantId, cmd.ActionUserId, cmd.ActionUserType);

        // Idempotency: check whether order already exists.
        var existing = await _dbContext.Orders.FindAsync([cmd.OrderId], context.CancellationToken);

        if (existing is not null)
        {
            _logger.LogWarning("Duplicate PersistOrderCommand for Order {OrderId} — replaying OrderPersisted.", cmd.OrderId);
            await PublishOrderPersisted(context, cmd.OrderId);
            return;
        }

        var orderItems = cmd.Items.Select(i => new OrderItemData
        {
            VariantId = i.VariantId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Discount = i.Discount
        }).ToList();

        var command = new PlaceOrderCommand
        {
            BuyerId = cmd.BuyerId,
            OrderId = cmd.OrderId,
            OrderItems = orderItems
        };

        var order = Domain.Aggregates.Orders.Order.CreateOrder(command);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Order {OrderId} persisted.", cmd.OrderId);

        await PublishOrderPersisted(context, cmd.OrderId);
    }

    private async Task PublishOrderPersisted(ConsumeContext context, Guid orderId)
    {
        await context.Publish(new OrderPersisted
        {
            OrderId = orderId,
            PersistedAt = DateTimeOffset.UtcNow
        }, context.CancellationToken);
    }
}
