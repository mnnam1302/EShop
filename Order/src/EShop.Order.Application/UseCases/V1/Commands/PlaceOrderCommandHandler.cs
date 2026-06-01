using EShop.Order.Domain.Commands;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS.Command;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Application.UseCases.V1.Commands;

public sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, Guid>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IPublishEndpoint publishEndpoint,
        IUserDetailsProvider userDetailsProvider,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid();
        var user = _userDetailsProvider.AuthenticatedUser;

        await _publishEndpoint.Publish(new OrderSubmitted
        {
            OrderId = orderId,
            BuyerId = command.BuyerId,
            Items = command.OrderItems.Select(i => new OrderItem
            {
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount
            }).ToList(),
            SubmittedAt = DateTimeOffset.UtcNow,
            TenantId = user.TenantId,
            ActionUserId = user.ActionUserId,
            ActionUserType = user.ActionUserType
        }, cancellationToken);

        _logger.LogInformation("Order {OrderId} submitted to saga.", orderId);

        return Result.Success(orderId);
    }
}
