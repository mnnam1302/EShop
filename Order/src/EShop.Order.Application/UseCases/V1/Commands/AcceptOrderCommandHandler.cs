using EShop.Order.Domain.Commands;
using EShop.Order.Domain.Repositories;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;

namespace EShop.Order.Application.UseCases.V1.Commands;

internal sealed class AcceptOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AcceptOrderCommand>
{
    public async Task<Result> HandleAsync(AcceptOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.FindSingleAsync(o => o.Id == command.OrderId, cancellationToken: cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order {command.OrderId} is not found.");
        }

        order.Accept();

        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
