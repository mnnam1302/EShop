using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;

namespace EShop.Catalog.Application.Categories.Create;

public sealed class CreateCategoryCommand : ICommand
{
    public string Name { get; set; }
    public string Reference { get; set; }
    public string Slug { get; set; }
    public Guid? ParentId { get; set; }
}

public sealed class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand>
{
    private readonly IEventStoreGateway eventStore;
    private readonly IUserDetailsProvider userDetailsProvider;

    public CreateCategoryCommandHandler(IEventStoreGateway eventStore, IUserDetailsProvider userDetailsProvider)
    {
        this.eventStore = eventStore;
        this.userDetailsProvider = userDetailsProvider;
    }

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

        return Result.Success();
    }
}