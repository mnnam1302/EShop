using EShop.Catalog.Application.Products.AddVariant;
using EShop.Catalog.Application.Products.AddVariationDimension;
using EShop.Catalog.Application.Products.ChangeVariantPrice;
using EShop.Catalog.Application.Products.ChangeVariationDimensionValues;
using EShop.Catalog.Application.Products.Create;
using EShop.Catalog.Application.Products.Delete;
using EShop.Catalog.Application.Products.Publish;
using EShop.Catalog.Application.Products.PublishVariant;
using EShop.Catalog.Application.Products.Unpublish;
using EShop.Catalog.Application.Products.UnpublishVariant;
using EShop.Catalog.Application.Products.Update;
using EShop.Catalog.Application.Products.UpdateVariant;
using EShop.Catalog.Application.Products.UpdateVariationDimension;

namespace EShop.Catalog.Application.Products;

public sealed partial class ProductAggregate
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
        LastModifiedAtUtc = @event.TimeStampUtc;
        TenantId = @event.TenantId;
        Scope = @event.Scope;
    }

    internal void Apply(ProductUpdatedEvent @event)
    {
        State.Fire(ProductAction.Update);

        Name = @event.Name;
        Description = @event.Description;
        CategoryId = @event.CategoryId;
        Tags = @event.Tags;
        Slug = @event.Slug;
        Images = @event.Images;
        Groups = @event.Groups;
        LastModifiedAtUtc = @event.UpdatedAtUtc;
        LastModifiedByUserId = @event.UpdatedByUserId;
    }

    internal void Apply(ProductPublishedEvent @event)
    {
        State.Fire(ProductAction.Publish);
        LastModifiedAtUtc = @event.PublishedAtUtc;
        LastModifiedByUserId = @event.PublishedByUserId;
    }

    internal void Apply(ProductUnpublishedEvent @event)
    {
        State.Fire(ProductAction.Unpublish);
        LastModifiedAtUtc = @event.UnpublishedAtUtc;
        LastModifiedByUserId = @event.UnpublishedByUserId;
    }

    internal void Apply(ProductDeletedEvent @event)
    {
        State.Fire(ProductAction.Delete);
        LastModifiedAtUtc = @event.DeletedAtUtc;
        LastModifiedByUserId = @event.DeletedByUserId;
    }

    internal void Apply(VariantCreatedEvent @event)
    {
        State.Fire(ProductAction.AddVariant);

        Variants.Add(new Variant
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
        });
    }

    internal void Apply(VariantUpdatedEvent @event)
    {
        State.Fire(ProductAction.UpdateVariant);

        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.Name = @event.Name;
        variant.Sku = @event.Sku;
    }

    internal void Apply(VariantPriceChangedEvent @event)
    {
        State.Fire(ProductAction.ChangeVariantPrice);

        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.Price = @event.NewPrice;
        variant.DiscountPrice = @event.NewDiscountPrice;
    }

    internal void Apply(VariantPublishedEvent @event)
    {
        State.Fire(ProductAction.PublishVariant);

        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.State = VariantState.Published;
    }

    internal void Apply(VariantUnpublishedEvent @event)
    {
        State.Fire(ProductAction.UnpublishVariant);

        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.State = VariantState.Unpublished;
    }

    internal void Apply(VariationDimensionAddedEvent @event)
    {
        State.Fire(ProductAction.AddVariationDimension);

        VariationDimensions.Add(new VariationDimension
        {
            Name = @event.Name,
            DisplayName = @event.DisplayName,
            Values = @event.Values,
            DisplayStyle = Enum.Parse<VariationDisplayStyles>(@event.DisplayStyle)
        });
    }

    internal void Apply(VariationDimensionUpdatedEvent @event)
    {
        State.Fire(ProductAction.UpdateVariationDimension);

        var dimension = VariationDimensions.First(d => string.Equals(d.Name, @event.Name, StringComparison.OrdinalIgnoreCase));
        dimension.DisplayName = @event.DisplayName;
        dimension.DisplayStyle = Enum.Parse<VariationDisplayStyles>(@event.DisplayStyle);
    }

    internal void Apply(VariationDimensionValuesChangedEvent @event)
    {
        State.Fire(ProductAction.ChangeVariationDimensionValues);

        var dimension = VariationDimensions.First(d => string.Equals(d.Name, @event.DimensionName, StringComparison.OrdinalIgnoreCase));
        dimension.Values = @event.Values;
    }
}