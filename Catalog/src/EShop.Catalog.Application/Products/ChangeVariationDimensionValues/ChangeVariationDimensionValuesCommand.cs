using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Products.ChangeVariationDimensionValues;

public sealed class ChangeVariationDimensionValuesCommand : ICommand
{
    public required Guid ProductId { get; set; }
    public required string DimensionName { get; set; }
    public required string[] Values { get; set; }
}

public sealed class ChangeVariationDimensionValuesCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<ChangeVariationDimensionValuesCommand>
{
    public async Task<Result> HandleAsync(ChangeVariationDimensionValuesCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.ProductId}' was not found."));
        }

        product.ChangeVariationDimensionValues(command);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync(new VariationDimensionValuesChanged
        {
            ProductId = product.Id,
            DimensionName = command.DimensionName,
            Values = command.Values,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}