using EShop.Order.Domain.Commands;
using EShop.Order.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Order.Application.UseCases.V1.Commands;

internal sealed class RejectOrderCommandHandler : ICommandHandler<RejectOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(RejectOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.FindSingleAsync(o => o.Id == command.OrderId, cancellationToken: cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order {command.OrderId} is not found.");
        }

        order.Reject(command.Reason);

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
