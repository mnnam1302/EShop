using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Products.UpdateVariant;

public sealed class UpdateVariantCommand : ICommand
{
    public required Guid ProductId { get; init; }
    public required Guid VariantId { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string Sku { get; init; } = string.Empty;
}

public sealed class UpdateVariantCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<UpdateVariantCommand>
{
    public async Task<Result> HandleAsync(UpdateVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.ProductId}' was not found."));
        }

        product.UpdateVariant(command.VariantId, command.Name, command.Sku, userDetailsProvider);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync(new VariantUpdated
        {
            ProductId = product.Id,
            VariantId = command.VariantId,
            Name = command.Name,
            Sku = command.Sku,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}
