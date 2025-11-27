using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
namespace EShop.Catalog.Application.Categories.Create;

public sealed class Command : ICommand
{
    public required string Name { get; set; }
    public required string Reference { get; set; }
    public required string Slug { get; set; }
    public Guid? ParentId { get; set; }
}

public sealed class CommandHandler : ICommandHandler<Command>
{
    private readonly IEventStoreGateway eventStore;
    private readonly IUserDetailsProvider userDetailsProvider;

    public CommandHandler(IEventStoreGateway eventStore, IUserDetailsProvider userDetailsProvider)
    {
        this.eventStore = eventStore;
        this.userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        if (command.ParentId.HasValue && command.ParentId != Guid.Empty)
        {
            var parentCategory = await eventStore.LoadAggregateAsync<Category>(command.ParentId.Value, cancellationToken);
            if (parentCategory == null)
            {
                return Result.Failure(new("ParentId", "Parent category not found"));
            }
        }

        var category = Category.Create(command, userDetailsProvider);

        await eventStore.AppendEventsAsync(category, cancellationToken);

        return Result.Success();
    }
}
