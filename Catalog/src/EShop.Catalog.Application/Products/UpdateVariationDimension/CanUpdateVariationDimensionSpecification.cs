using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.UpdateVariationDimension;

public sealed class CanUpdateVariationDimensionSpecification : Specification<ProductAggregate>
{
    private readonly string _name;

    private CanUpdateVariationDimensionSpecification(string name)
    {
        _name = name;
    }

    public static CanUpdateVariationDimensionSpecification New(string name) => new(name);

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.UpdateVariationDimension))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot update variation dimension";
            yield break;
        }

        if (!product.VariationDimensions.Any(d => string.Equals(d.Name, _name, StringComparison.OrdinalIgnoreCase)))
        {
            yield return $"dimension '{_name}' does not exist";
        }
    }
}
