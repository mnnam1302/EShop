using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Products.Delete;

public sealed class DeleteProductCommand(Guid id) : ICommand
{
    public Guid Id { get; init; } = id;
}

public sealed class DeleteProductCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> HandleAsync(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.Id}' was not found."));
        }

        product.Delete(userDetailsProvider);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync(new ProductDeleted
        {
            ProductId = product.Id,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}
