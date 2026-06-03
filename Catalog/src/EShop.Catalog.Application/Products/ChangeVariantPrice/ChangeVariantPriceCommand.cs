using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Products.ChangeVariantPrice;

public sealed class ChangeVariantPriceCommand : ICommand
{
    public required Guid ProductId { get; init; }
    public required Guid VariantId { get; init; }
    public required decimal Price { get; init; }
    public required decimal DiscountPrice { get; init; }
}

public sealed class ChangeVariantPriceCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<ChangeVariantPriceCommand>
{
    public async Task<Result> HandleAsync(ChangeVariantPriceCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.ProductId}' was not found."));
        }

        var variant = product.Variants.First(v => v.Id == command.VariantId);
        var oldPrice = variant.Price;
        var oldDiscountPrice = variant.DiscountPrice;

        product.ChangeVariantPrice(command, userDetailsProvider);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync(new VariantPriceChanged
        {
            ProductId = product.Id,
            VariantId = command.VariantId,
            OldPrice = oldPrice,
            NewPrice = command.Price,
            OldDiscountPrice = oldDiscountPrice,
            NewDiscountPrice = command.DiscountPrice,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}