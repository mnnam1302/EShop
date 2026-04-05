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
        }
        else if (variant.State == VariantState.Deleted)
        {
            yield return $"variant '{_variantId}' is in Deleted state";
        }
    }
}
