using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.UpdateVariant;

public sealed class CanUpdateVariantSpecification : Specification<ProductAggregate>
{
    private readonly Guid _variantId;

    private CanUpdateVariantSpecification(Guid variantId)
    {
        _variantId = variantId;
    }

    public static CanUpdateVariantSpecification New(Guid variantId) => new(variantId);

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.UpdateVariant))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot update variant";
            yield break;
        }

        var variant = product.Variants.FirstOrDefault(v => v.Id == _variantId);
        if (variant is null)
        {
            yield return $"variant '{_variantId}' does not exist";
        }
        else if (variant.State == VariantState.Deleted)
        {
            yield return $"variant '{_variantId}' is in Deleted state";
        }
    }
}
