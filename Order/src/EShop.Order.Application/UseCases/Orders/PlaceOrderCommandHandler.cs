using EShop.Order.Domain.Commands;
using EShop.Order.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Order.Application.UseCases.Orders;

public sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand>
{
    private readonly ILogger<PlaceOrderCommandHandler> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderCommandHandler(
        ILogger<PlaceOrderCommandHandler> logger,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Domain.Aggregates.Order.CreateOrder(command);

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class OrderDto
{
    public required Guid Id { get; init; }
    public required string BuyerId { get; init; }
    public required DateTimeOffset OrderDate { get; init; }
    public required string Status { get; init; }
    public string? Description { get; init; }
    public required List<OrderItemDto> OrderItems { get; init; }

    public static OrderDto MapFrom(Domain.Aggregates.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Description = order.Description,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                VariantId = oi.VariantId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Discount = oi.Discount
            }).ToList()
        };
    }
}

public sealed class OrderItemDto
{
    public required Guid Id { get; init; }
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public decimal? Discount { get; init; }
}
