using EShop.Catalog.Application.Categories;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace EShop.Catalog.Application.Products.Create;

public sealed class CreateProductCommand : ICommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public double Price { get; set; }
    public double DiscountPrice { get; set; }
    public IEnumerable<string> Tags { get; set; } = [];
    public string Slug { get; set; } = string.Empty;
    public IEnumerable<string> Images { get; set; } = [];
    public IEnumerable<Guid> Groups { get; set; } = [];
}

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IEventStoreGateway eventStore;
    private readonly IEventBus eventBus;
    private readonly IUserDetailsProvider userDetailsProvider;

    public CreateProductCommandHandler(
        IEventStoreGateway eventStore,
        IEventBus eventBus,
        ILogger<CreateProductCommandHandler> logger,
        IUserDetailsProvider userDetailsProvider)
    {
        this.eventStore = eventStore;
        this.eventBus = eventBus;
        this.userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var category = await eventStore.LoadAggregateAsync<CategoryAggregate>(command.CategoryId, cancellationToken);
        if (category is null)
        {
            return ValidationResult.WithError(Error.Create("Category.NotFound", $"Category with ID '{command.CategoryId}' was not found."));
        }

        var product = ProductAggregate.Create(command, userDetailsProvider);

        product.AddVariant(string.Empty, string.Empty, command.Price, command.DiscountPrice, [], true);

        await eventStore.AppendEventsAsync(product, cancellationToken);

        await eventBus.PublishAsync<ProductCreated>(new
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Tags = product.Tags,
            Slug = product.Slug,
            Images = product.Images,
            Groups = product.Groups,
            CreatedByUserId = product.CreatedByUserId,
            CreatedAtUtc = product.CreatedAtUtc,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}
