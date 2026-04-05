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

        return product;
    }

    internal void Update(UpdateProductCommand command, IUserDetailsProvider userDetailsProvider)
    {
        ProductCanUpdateSpecification.New().ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new ProductUpdatedEvent
        {
            ProductId = command.Id,
            Name = command.Name,
            Description = command.Description,
            CategoryId = command.CategoryId,
            Tags = command.Tags.ToArray(),
            Slug = command.Slug,
            Images = command.Images.ToArray(),
            Groups = command.Groups.ToArray(),
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void Publish(IUserDetailsProvider userDetailsProvider)
    {
        ProductCanPublishSpecification.New().ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new ProductPublishedEvent
        {
            ProductId = Id,
            PublishedAtUtc = DateTimeOffset.UtcNow,
            PublishedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void Unpublish(IUserDetailsProvider userDetailsProvider)
    {
        ProductCanUnpublishSpecification.New().ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new ProductUnpublishedEvent
        {
            ProductId = Id,
            UnpublishedAtUtc = DateTimeOffset.UtcNow,
            UnpublishedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void Delete(IUserDetailsProvider userDetailsProvider)
    {
        ProductCanDeleteSpecification.New().ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new ProductDeletedEvent
        {
            ProductId = Id,
            DeletedAtUtc = DateTimeOffset.UtcNow,
            DeletedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void AddVariant(Guid variantId, string name, string sku, decimal price, decimal discountPrice, VariantDimensionValue[] values, bool isDefault)
    {
        ProductCanAddVariantSpecification.New(isDefault, values.ToList())
            .ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantCreatedEvent
        {
            VariantId = variantId,
            ProductId = Id,
            Name = name,
            Sku = sku,
            Price = price,
            DiscountPrice = discountPrice,
            VariantDimensionValues = values.ToList(),
            IsDefault = isDefault
        });
    }

    internal void AddVariationDimension(string name, string displayName, string[] values, string displayStyle)
    {
        CanAddVariationDimensionSpecification.New(name, values).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariationDimensionAddedEvent
        {
            ProductId = Id,
            Name = name,
            DisplayName = displayName,
            Values = values,
            DisplayStyle = displayStyle
        });
    }

    internal void UpdateVariationDimension(string name, string displayName, string displayStyle)
    {
        CanUpdateVariationDimensionSpecification.New(name).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariationDimensionUpdatedEvent
        {
            ProductId = Id,
            Name = name,
            DisplayName = displayName,
            DisplayStyle = displayStyle
        });
    }

    internal void ChangeVariationDimensionValues(string dimensionName, string[] values)
    {
        CanChangeVariationDimensionValuesSpecification.New(dimensionName, values).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariationDimensionValuesChangedEvent
        {
            ProductId = Id,
            DimensionName = dimensionName,
            Values = values
        });
    }

    internal void UpdateVariant(Guid variantId, string name, string sku, IUserDetailsProvider userDetailsProvider)
    {
        CanUpdateVariantSpecification.New(variantId).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantUpdatedEvent
        {
            ProductId = Id,
            VariantId = variantId,
            Name = name,
            Sku = sku,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void ChangeVariantPrice(Guid variantId, decimal price, decimal discountPrice, IUserDetailsProvider userDetailsProvider)
    {
        CanChangeVariantPriceSpecification.New(variantId, price, discountPrice).ThrowDomainErrorIfNotSatisfied(this);

        var variant = Variants.First(v => v.Id == variantId);

        RaiseEvent(new VariantPriceChangedEvent
        {
            ProductId = Id,
            VariantId = variantId,
            OldPrice = variant.Price,
            NewPrice = price,
            OldDiscountPrice = variant.DiscountPrice,
            NewDiscountPrice = discountPrice,
            ChangedAtUtc = DateTimeOffset.UtcNow,
            ChangedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void PublishVariant(Guid variantId, IUserDetailsProvider userDetailsProvider)
    {
        CanPublishVariantSpecification.New(variantId).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantPublishedEvent
        {
            ProductId = Id,
            VariantId = variantId,
            PublishedAtUtc = DateTimeOffset.UtcNow,
            PublishedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void UnpublishVariant(Guid variantId, IUserDetailsProvider userDetailsProvider)
    {
        CanUnpublishVariantSpecification.New(variantId).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantUnpublishedEvent
        {
            ProductId = Id,
            VariantId = variantId,
            UnpublishedAtUtc = DateTimeOffset.UtcNow,
            UnpublishedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
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

    internal void Apply(VariantCreatedEvent @event)
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

    internal void Apply(VariationDimensionAddedEvent @event)
    {
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
        var dimension = VariationDimensions.First(d => string.Equals(d.Name, @event.Name, StringComparison.OrdinalIgnoreCase));
        dimension.DisplayName = @event.DisplayName;
        dimension.DisplayStyle = Enum.Parse<VariationDisplayStyles>(@event.DisplayStyle);
    }

    internal void Apply(VariationDimensionValuesChangedEvent @event)
    {
        var dimension = VariationDimensions.First(d => string.Equals(d.Name, @event.DimensionName, StringComparison.OrdinalIgnoreCase));
        dimension.Values = @event.Values;
    }

    internal void Apply(VariantUpdatedEvent @event)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.Name = @event.Name;
        variant.Sku = @event.Sku;
    }

    internal void Apply(VariantPriceChangedEvent @event)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.Price = @event.NewPrice;
        variant.DiscountPrice = @event.NewDiscountPrice;
    }

    internal void Apply(VariantPublishedEvent @event)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.State = VariantState.Published;
    }

    internal void Apply(VariantUnpublishedEvent @event)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId);
        variant.State = VariantState.Unpublished;
    }

    #endregion Apply Domain Event (Replay technique in Domain-Driven Design)
}