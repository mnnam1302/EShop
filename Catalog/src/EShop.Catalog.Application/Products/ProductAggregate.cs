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

public sealed partial class ProductAggregate : Aggregate, IAuditable, IScoped, IRingFenced
{
    public string TenantId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

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

    internal void AddDefaultVariant(decimal price, decimal discountPrice)
    {
        ProductCanAddVariantSpecification.New(true, [])
            .ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantCreatedEvent
        {
            VariantId = Guid.NewGuid(),
            ProductId = Id,
            Name = string.Empty,
            Sku = string.Empty,
            Price = price,
            DiscountPrice = discountPrice,
            VariantDimensionValues = [],
            IsDefault = true
        });
    }

    internal void AddVariant(AddVariantCommand command)
    {
        var dimensionValues = command.DimensionValues
            .Select(dv => new VariantDimensionValue { Name = dv.Name, Value = dv.Value })
            .ToList();

        ProductCanAddVariantSpecification.New(false, dimensionValues)
            .ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantCreatedEvent
        {
            VariantId = Guid.NewGuid(),
            ProductId = Id,
            Name = command.Name,
            Sku = command.Sku,
            Price = command.Price,
            DiscountPrice = command.DiscountPrice,
            VariantDimensionValues = dimensionValues,
            IsDefault = false
        });
    }

    internal void AddVariationDimension(AddVariationDimensionCommand command)
    {
        CanAddVariationDimensionSpecification.New(command.Name, command.Values).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariationDimensionAddedEvent
        {
            ProductId = Id,
            Name = command.Name,
            DisplayName = command.DisplayName,
            Values = command.Values,
            DisplayStyle = command.DisplayStyle
        });
    }

    internal void UpdateVariationDimension(UpdateVariationDimensionCommand command)
    {
        CanUpdateVariationDimensionSpecification.New(command.Name).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariationDimensionUpdatedEvent
        {
            ProductId = Id,
            Name = command.Name,
            DisplayName = command.DisplayName,
            DisplayStyle = command.DisplayStyle
        });
    }

    internal void ChangeVariationDimensionValues(ChangeVariationDimensionValuesCommand command)
    {
        CanChangeVariationDimensionValuesSpecification.New(command.DimensionName, command.Values).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariationDimensionValuesChangedEvent
        {
            ProductId = Id,
            DimensionName = command.DimensionName,
            Values = command.Values
        });
    }

    internal void UpdateVariant(UpdateVariantCommand command, IUserDetailsProvider userDetailsProvider)
    {
        CanUpdateVariantSpecification.New(command.VariantId).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantUpdatedEvent
        {
            ProductId = Id,
            VariantId = command.VariantId,
            Name = command.Name,
            Sku = command.Sku,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void ChangeVariantPrice(ChangeVariantPriceCommand command, IUserDetailsProvider userDetailsProvider)
    {
        CanChangeVariantPriceSpecification.New(command.VariantId, command.Price, command.DiscountPrice).ThrowDomainErrorIfNotSatisfied(this);

        var variant = Variants.First(v => v.Id == command.VariantId);

        RaiseEvent(new VariantPriceChangedEvent
        {
            ProductId = Id,
            VariantId = command.VariantId,
            OldPrice = variant.Price,
            NewPrice = command.Price,
            OldDiscountPrice = variant.DiscountPrice,
            NewDiscountPrice = command.DiscountPrice,
            ChangedAtUtc = DateTimeOffset.UtcNow,
            ChangedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void PublishVariant(PublishVariantCommand command, IUserDetailsProvider userDetailsProvider)
    {
        CanPublishVariantSpecification.New(command.VariantId).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantPublishedEvent
        {
            ProductId = Id,
            VariantId = command.VariantId,
            PublishedAtUtc = DateTimeOffset.UtcNow,
            PublishedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }

    internal void UnpublishVariant(UnpublishVariantCommand command, IUserDetailsProvider userDetailsProvider)
    {
        CanUnpublishVariantSpecification.New(command.VariantId).ThrowDomainErrorIfNotSatisfied(this);

        RaiseEvent(new VariantUnpublishedEvent
        {
            ProductId = Id,
            VariantId = command.VariantId,
            UnpublishedAtUtc = DateTimeOffset.UtcNow,
            UnpublishedByUserId = userDetailsProvider.AuthenticatedUser.ActionUserId
        });
    }
}