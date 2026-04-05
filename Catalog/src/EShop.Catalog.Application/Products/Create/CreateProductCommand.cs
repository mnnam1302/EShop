using EShop.Catalog.Application.Categories;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.EventBus.Abstractions;

namespace EShop.Catalog.Application.Products.Create;

public sealed class CreateProductCommand : ICommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public decimal Price { get; init; }
    public decimal DiscountPrice { get; init; }
    public IEnumerable<string> Tags { get; init; } = [];
    public string Slug { get; init; } = string.Empty;
    public IEnumerable<string> Images { get; init; } = [];
    public IEnumerable<Guid> Groups { get; init; } = [];
}

public sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IAggregateStore aggregateStore;
    private readonly IEventBus eventBus;
    private readonly IUserDetailsProvider userDetailsProvider;

    public CreateProductCommandHandler(
        IAggregateStore aggregateStore,
        IEventBus eventBus,
        ILogger<CreateProductCommandHandler> logger,
        IUserDetailsProvider userDetailsProvider)
    {
        this.aggregateStore = aggregateStore;
        this.eventBus = eventBus;
        this.userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var category = await aggregateStore.LoadAggregateAsync<CategoryAggregate>(command.CategoryId, cancellationToken);
        if (category is null)
        {
            return ValidationResult.WithError(Error.Create("Category.NotFound", $"Category {command.CategoryId}' is not found."));
        }

        var product = ProductAggregate.Create(command, userDetailsProvider);
        product.AddDefaultVariant(command.Price, command.DiscountPrice);

        await aggregateStore.AppendEventsAsync(product, cancellationToken);

        var defaultVariant = product.Variants.First(variant => variant.IsDefault);
        await eventBus.PublishAsync(new ProductCreated
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Tags = product.Tags,
            Slug = product.Slug,
            Images = product.Images,
            Groups = product.Groups,
            DefaultVariant = new ProductDefaultVariant
            {
                VariantId = defaultVariant.Id,
                Price = defaultVariant.Price,
                DiscountPrice = defaultVariant.DiscountPrice
            },
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }
}