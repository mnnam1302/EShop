using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;

namespace EShop.Catalog.Application.Products.UnpublishVariant;

public sealed class UnpublishVariantCommand : ICommand
{
    public required Guid ProductId { get; init; }
    public required Guid VariantId { get; init; }
}

public sealed class UnpublishVariantCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<UnpublishVariantCommand>
{
    public async Task<Result> HandleAsync(UnpublishVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.ProductId}' was not found."));
        }

        product.UnpublishVariant(command, userDetailsProvider);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync(new VariantUnpublished
        {
            ProductId = product.Id,
            VariantId = command.VariantId,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}