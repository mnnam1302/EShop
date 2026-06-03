using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Products.AddVariant;

public sealed class AddVariantCommand : ICommand
{
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public required string Sku { get; init; }
    public required decimal Price { get; init; }
    public required decimal DiscountPrice { get; init; }
    public required List<VariantDimensionValueInput> DimensionValues { get; init; }
}

public sealed class VariantDimensionValueInput
{
    public required string Name { get; init; }
    public required string Value { get; init; }
}

public sealed class AddVariantCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<AddVariantCommand>
{
    public async Task<Result> HandleAsync(AddVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.ProductId}' was not found."));
        }

        product.AddVariant(command);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        var createdVariant = product.Variants.Last();
        await eventBus.PublishAsync(new VariantCreated
        {
            ProductId = product.Id,
            VariantId = createdVariant.Id,
            Name = createdVariant.Name,
            Sku = createdVariant.Sku,
            Price = createdVariant.Price,
            DiscountPrice = createdVariant.DiscountPrice,
            IsDefault = false,
            VariantDimensionValues = createdVariant.VariantDimensionValues
                .Select(dv => new VariantDimensionValueData { Name = dv.Name, Value = dv.Value })
                .ToList(),
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}
