using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Products.UpdateVariationDimension;

public sealed class UpdateVariationDimensionCommand : ICommand
{
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string DisplayStyle { get; init; }
}

public sealed class UpdateVariationDimensionCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<UpdateVariationDimensionCommand>
{
    public async Task<Result> HandleAsync(UpdateVariationDimensionCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.ProductId}' was not found."));
        }

        product.UpdateVariationDimension(command);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync(new VariationDimensionUpdated
        {
            ProductId = product.Id,
            Name = command.Name,
            DisplayName = command.DisplayName,
            DisplayStyle = command.DisplayStyle,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}