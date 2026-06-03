using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;

namespace EShop.Catalog.Application.Products.PublishVariant;

public sealed class PublishVariantCommand : ICommand
{
    public required Guid ProductId { get; init; }
    public required Guid VariantId { get; init; }
}

public sealed class PublishVariantCommandHandler(
    IAggregateStore aggregateStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<PublishVariantCommand>
{
    public async Task<Result> HandleAsync(PublishVariantCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.ProductId}' was not found."));
        }

        product.PublishVariant(command, userDetailsProvider);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync(new VariantPublished
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
