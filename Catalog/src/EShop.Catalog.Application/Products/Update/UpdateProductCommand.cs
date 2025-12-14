using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;

namespace EShop.Catalog.Application.Products.Update;

public sealed class UpdateProductCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string[] Tags { get; set; } = [];
    public string Slug { get; set; } = string.Empty;
    public string[] Images { get; set; } = [];
    public Guid[] Groups { get; set; } = [];
}

public sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private readonly IAggregateStore aggregateStore;
    private readonly IUserDetailsProvider userDetailsProvider;

    public UpdateProductCommandHandler(IAggregateStore aggregateStore, IUserDetailsProvider userDetailsProvider)
    {
        this.aggregateStore = aggregateStore;
        this.userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await aggregateStore.LoadAggregateAsync<ProductAggregate>(command.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure(new Error("ProductNotFound", $"Product with Id '{command.Id}' was not found."));
        }

        product.Update(command, userDetailsProvider);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        return Result.Success();
    }
}
