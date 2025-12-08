using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Catalog.Application.Categories.Unpublish
{
    public sealed class UnpublishCategoryCommand(Guid id) : ICommand
    {
        public Guid Id { get; set; } = id;
    }

    public sealed class UnpublishCategoryCommandHandler : ICommandHandler<UnpublishCategoryCommand>
    {
        private readonly IEventStoreGateway eventStore;

        public UnpublishCategoryCommandHandler(IEventStoreGateway eventStore)
        {
            this.eventStore = eventStore;
        }

        public async Task<Result> HandleAsync(UnpublishCategoryCommand command, CancellationToken cancellationToken)
        {
            var category = await eventStore.LoadAggregateAsync<CategoryAggregate>(command.Id, cancellationToken);
            if (category is null)
            {
                throw new NotFoundException($"Category '{command.Id}' was not found.");
            }

            category.Unpublish();

            await eventStore.AppendEventsAsync(category, cancellationToken);
            return Result.Success();
        }
    }
}
