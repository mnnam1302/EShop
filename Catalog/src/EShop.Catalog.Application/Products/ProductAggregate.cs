using EShop.Catalog.Application.Products.Create;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products;

public sealed class ProductAggregate : Aggregate, IAuditable, IScoped, IRingFenced
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string[] Tags { get; private set; } = [];
    public string Slug { get; private set; } = string.Empty;
    public string[] Images { get; private set; } = [];
    public Guid[] Groups { get; private set; } = [];
    public ProductStateMachine State { get; private set; } = new();

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? LastModifiedByUserId { get; set; }
    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    public List<Variant> Variants { get; private set; } = [];
    public List<VariationDimension> VariationDimensions { get; private set; } = [];

    public string TenantId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

    #region Behaviors

    internal static ProductAggregate Create(CreateProductCommand command, IUserDetailsProvider userDetailsProvider)
    {
        var product = new ProductAggregate();

        product.RaiseEvent(new ProductCreatedEvent
        {
            ProductId = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            CategoryId = command.CategoryId,
            Tags = command.Tags.ToArray(),
            Slug = command.Slug,
            Images = command.Images.ToArray(),
            Groups = command.Groups.ToArray(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            Scope = userDetailsProvider.AuthenticatedUser.TenantId
        });

        product.AddVariant(string.Empty, string.Empty, command.Price, command.DiscountPrice, [], true);

        return product;
    }

    public void AddVariant(string name, string sku, double price, double discountPrice, IEnumerable<VariantDimensionValue> values, bool isDefault)
    {
        ProductCanAddVariantSpecification.New(isDefault, values.ToList())
            .ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantAddedEvent
        {
            ProductId = Id,
            VariantId = Guid.NewGuid(),
            Name = name,
            Sku = sku,
            Price = price,
            DiscountPrice = discountPrice,
            VariantDimensionValues = values.ToList(),
            IsDefault = isDefault
        });
    }

    #endregion Behaviors

    #region Apply Domain Event (Replay technique in Domain-Driven Design)

    internal void Apply(ProductCreatedEvent @event)
    {
        Id = @event.ProductId;
        Name = @event.Name;
        Description = @event.Description;
        CategoryId = @event.CategoryId;
        Tags = @event.Tags;
        Slug = @event.Slug;
        Images = @event.Images;
        Groups = @event.Groups;
        CreatedAtUtc = @event.CreatedAtUtc;
        CreatedByUserId = @event.CreatedByUserId;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
        LastModifiedAtUtc = @event.TimeStampUtc;
    }

    internal void Apply(VariantAddedEvent @event)
    {
        var variant = new Variant
        {
            Id = @event.VariantId,
            ProductId = @event.ProductId,
            Name = @event.Name,
            Sku = @event.Sku,
            Price = @event.Price,
            DiscountPrice = @event.DiscountPrice,
            IsDefault = @event.IsDefault,
            State = VariantState.Unpublished,
            VariantDimensionValues = @event.VariantDimensionValues,
        };

        Variants.Add(variant);
    }

    #endregion Apply Domain Event (Replay technique in Domain-Driven Design)
}