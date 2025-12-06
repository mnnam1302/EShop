using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Categories.Create;

public sealed class CreateCategoryCommand : ICommand
{
    public required string Name { get; set; }
    public required string Reference { get; set; }
    public required string Slug { get; set; }
    public Guid? ParentId { get; set; }
}

public sealed class CreateCategoryCommandHandler(
    IEventStoreGateway eventStore,
    IUserDetailsProvider userDetailsProvider,
    IEventBusGateway eventBus) : ICommandHandler<CreateCategoryCommand>
{
    public async Task<Result> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (command.ParentId.HasValue && command.ParentId != Guid.Empty)
        {
            var parentCategory = await eventStore.LoadAggregateAsync<CategoryAggregate>(command.ParentId.Value, cancellationToken);
            if (parentCategory == null)
            {
                return Result.Failure(new("ParentId", "Parent category not found"));
            }
        }

        var category = CategoryAggregate.Create(command, userDetailsProvider);

        await eventStore.AppendEventsAsync(category, cancellationToken);

        await eventBus.PublishAsync(new CategoryCreated
        {
            CategoryId = category.Id,
            Name = category.Name,
            Reference = category.Reference,
            Slug = category.Slug,
            ParentId = category.ParentId,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType,
        }, cancellationToken);

        return Result.Success();
    }
}