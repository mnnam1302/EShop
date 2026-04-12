using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.UnpublishVariant;

public sealed class CanUnpublishVariantSpecification : Specification<ProductAggregate>
{
    private readonly Guid _variantId;

    private CanUnpublishVariantSpecification(Guid variantId)
    {
        _variantId = variantId;
    }

    public static CanUnpublishVariantSpecification New(Guid variantId) => new(variantId);

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.UnpublishVariant))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot unpublish variant";
            yield break;
        }

        var variant = product.Variants.FirstOrDefault(v => v.Id == _variantId);
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

        if (variant.State != VariantState.Published)
        {
            yield return $"variant '{_variantId}' is not in Published state";
            yield break;
        }

        if (product.State.IsInState(ProductState.Published))
        {
            var publishedVariantCount = product.Variants.Count(v => v.State == VariantState.Published);
            if (publishedVariantCount <= 1)
            {
                yield return "the last published variant cannot be unpublished while the product is published";
            }
        }
    }
}
