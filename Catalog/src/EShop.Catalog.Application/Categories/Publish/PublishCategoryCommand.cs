using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Catalog.Application.Categories.Publish;

public sealed class PublishCategoryCommand(Guid categoryId) : ICommand
{
    public Guid Id { get; set; } = categoryId;
}

public sealed class PublishCategoryCommandHandler(IEventStoreGateway eventStore) : ICommandHandler<PublishCategoryCommand>
{
    public async Task<Result> HandleAsync(PublishCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await eventStore.LoadAggregateAsync<CategoryAggregate>(command.Id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException($"Category {command.Id} is not found.");
        }

        category.Publish();

        await eventStore.AppendEventsAsync(category, cancellationToken);
        return Result.Success();
    }
}