using EShop.Shared.Contracts.Services.Catalog;
using EShop.Shared.ReadModel;

namespace EShop.Catalog.ReadModels.MongoDb.Models;

public sealed partial class Product :
    IAmReadModelFor<ProductCreated>,
    IAmReadModelFor<ProductUpdated>,
    IAmReadModelFor<ProductPublished>,
    IAmReadModelFor<ProductUnpublished>,
    IAmReadModelFor<ProductDeleted>,
    IAmReadModelFor<VariationDimensionAdded>,
    IAmReadModelFor<VariationDimensionUpdated>,
    IAmReadModelFor<VariationDimensionValuesChanged>,
    IAmReadModelFor<VariantCreated>,
    IAmReadModelFor<VariantUpdated>,
    IAmReadModelFor<VariantPriceChanged>,
    IAmReadModelFor<VariantPublished>,
    IAmReadModelFor<VariantUnpublished>
{
    public void Apply(ProductCreated @event, IReadModelContext context)
    {
        var defaultVariant = @event.DefaultVariant;

        Id = @event.ProductId.ToString();
        DocumentId = @event.ProductId;
        Name = @event.Name;
        Description = @event.Description;
        Slug = @event.Slug;
        CategoryId = @event.CategoryId.ToString();
        Tags = @event.Tags;
        Images = @event.Images;
        State = "Draft";
        CreatedByUserId = @event.ActionUserId;
        CreatedAtUtc = @event.TimeStampUtc;
        LastModifiedByUserId = @event.ActionUserId;
        LastModifiedAtUtc = @event.TimeStampUtc;
        TenantId = @event.TenantId;
        Scope = @event.TenantId;
        Variants.Add(new ProductVariant
        {
            Id = defaultVariant.VariantId.ToString(),
            Name = defaultVariant.Name,
            Sku = defaultVariant.Sku,
            Price = defaultVariant.Price,
            DiscountPrice = defaultVariant.DiscountPrice,
            IsDefault = defaultVariant.IsDefault,
            State = defaultVariant.State,
            DimensionValues = []
        });
    }

    public void Apply(ProductUpdated @event, IReadModelContext context)
    {
        Name = @event.Name;
        Description = @event.Description;
        Slug = @event.Slug;
        CategoryId = @event.CategoryId.ToString();
        Tags = @event.Tags;
        Images = @event.Images;
        LastModifiedByUserId = @event.ActionUserId;
        LastModifiedAtUtc = @event.TimeStampUtc;
    }

    public void Apply(ProductPublished @event, IReadModelContext context)
    {
        State = "Published";
        LastModifiedByUserId = @event.ActionUserId;
        LastModifiedAtUtc = @event.TimeStampUtc;
    }

    public void Apply(ProductUnpublished @event, IReadModelContext context)
    {
        State = "Unpublished";
        LastModifiedByUserId = @event.ActionUserId;
        LastModifiedAtUtc = @event.TimeStampUtc;
    }

    public void Apply(ProductDeleted @event, IReadModelContext context)
    {
        State = "Deleted";
        LastModifiedByUserId = @event.ActionUserId;
        LastModifiedAtUtc = @event.TimeStampUtc;
    }

    public void Apply(VariationDimensionAdded @event, IReadModelContext context)
    {
        VariationDimensions.Add(new ProductVariationDimension
        {
            Name = @event.Name,
            DisplayName = @event.DisplayName,
            Values = @event.Values,
            DisplayStyle = @event.DisplayStyle
        });
    }

    public void Apply(VariationDimensionUpdated @event, IReadModelContext context)
    {
        var dimension = VariationDimensions.First(d =>
            string.Equals(d.Name, @event.Name, StringComparison.OrdinalIgnoreCase));

        dimension.DisplayName = @event.DisplayName;
        dimension.DisplayStyle = @event.DisplayStyle;
    }

    public void Apply(VariationDimensionValuesChanged @event, IReadModelContext context)
    {
        var dimension = VariationDimensions.First(d =>
            string.Equals(d.Name, @event.DimensionName, StringComparison.OrdinalIgnoreCase));

        dimension.Values = @event.Values;
    }

    public void Apply(VariantCreated @event, IReadModelContext context)
    {
        Variants.Add(new ProductVariant
        {
            Id = @event.VariantId.ToString(),
            Name = @event.Name,
            Sku = @event.Sku,
            Price = @event.Price,
            DiscountPrice = @event.DiscountPrice,
            IsDefault = @event.IsDefault,
            State = "Unpublished",
            DimensionValues = @event.VariantDimensionValues
                .Select(v => new ProductVariantDimensionValue
                {
                    Name = v.Name,
                    Value = v.Value
                }).ToList()
        });
    }

    public void Apply(VariantUpdated @event, IReadModelContext context)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId.ToString());
        variant.Name = @event.Name;
        variant.Sku = @event.Sku;
    }

    public void Apply(VariantPriceChanged @event, IReadModelContext context)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId.ToString());
        variant.Price = @event.NewPrice;
        variant.DiscountPrice = @event.NewDiscountPrice;
    }

    public void Apply(VariantPublished @event, IReadModelContext context)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId.ToString());
        variant.State = "Published";
    }

    public void Apply(VariantUnpublished @event, IReadModelContext context)
    {
        var variant = Variants.First(v => v.Id == @event.VariantId.ToString());
        variant.State = "Unpublished";
    }
}
