using EShop.Order.Domain.Commands;
using EShop.Order.Domain.Repositories;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Order;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Application.UseCases.V1.Commands;

public sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, Guid>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderCommandHandler(
        IPublishEndpoint publishEndpoint,
        IUserDetailsProvider userDetailsProvider,
        ILogger<PlaceOrderCommandHandler> logger,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _publishEndpoint = publishEndpoint;
        _userDetailsProvider = userDetailsProvider;
        _logger = logger;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var user = _userDetailsProvider.AuthenticatedUser;

        var order = Domain.Aggregates.Order.CreateOrder(command);

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderSubmitted
        {
            OrderId = order.Id,
            BuyerId = order.BuyerId,
            Items = order.OrderItems.Select(i => new OrderItem
            {
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount
            }).ToList(),
            SubmittedAt = order.OrderDate,
            TenantId = user.TenantId,
            ActionUserId = user.ActionUserId,
            ActionUserType = user.ActionUserType
        }, cancellationToken);

        _logger.LogInformation("Order {OrderId} submitted to saga.", order.Id);

        return Result.Success(order.Id);
    }
}
