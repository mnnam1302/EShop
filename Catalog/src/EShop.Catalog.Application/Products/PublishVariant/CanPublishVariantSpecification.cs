using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.PublishVariant;

public sealed class CanPublishVariantSpecification : Specification<ProductAggregate>
{
    private readonly Guid _variantId;

    private CanPublishVariantSpecification(Guid variantId)
    {
        _variantId = variantId;
    }

    public static CanPublishVariantSpecification New(Guid variantId) => new(variantId);

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (obj.State.IsInState(ProductState.Deleted))
        {
            yield return $"product {obj.Id} is in Deleted state";
        }

        var variant = obj.Variants.FirstOrDefault(v => v.Id == _variantId);
        if (variant is null)
        {
            yield return $"variant '{_variantId}' does not exist";
            yield break;
        }

        if (variant.State == VariantState.Deleted)
        {
            yield return $"variant '{_variantId}' is in Deleted state";
            yield break;
        }

        if (variant.State != VariantState.Unpublished)
        {
            yield return $"variant '{_variantId}' is already published";
            yield break;
        }

        if (string.IsNullOrEmpty(variant.Sku))
        {
            yield return "SKU is required";
        }

        if (variant.Price <= 0)
        {
            yield return "Price must be greater than zero";
        }

        if (!variant.IsDefault)
        {
            if (variant.VariantDimensionValues.Count != obj.VariationDimensions.Count)
            {
                yield return "all dimension values must be present";
            }
        }
    }
}
