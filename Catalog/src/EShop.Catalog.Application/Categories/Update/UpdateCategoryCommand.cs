using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Categories.Update;

public sealed class UpdateCategoryCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Reference { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public Guid? ParentId { get; set; }
}

public sealed class UpdateCategoryCommandHandler(
    IAggregateStore eventStore,
    IEventBus eventBus,
    IUserDetailsProvider userDetailsProvider) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> HandleAsync(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await eventStore.LoadAggregateAsync<CategoryAggregate>(command.Id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"Category {command.Id} is not found.");
        }

        category.Update(command);

        await eventStore.AppendEventsAsync(category, cancellationToken);

        await eventBus.PublishAsync<CategoryUpdated>(new
        {
            CategoryId = category.Id,
            Version = category.Version,
            Name = category.Name,
            Reference = category.Reference,
            Slug = category.Slug,
            ParentId = category.ParentId,
            CreatedAtUtc = category.CreatedAtUtc,
            UpdatedAtUtc = category.UpdatedAtUtc,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}
