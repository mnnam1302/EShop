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

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (obj.State.IsInState(ProductState.Deleted))
        {
            yield return $"product {obj.Id} is in Deleted state";
        }

        if (!obj.VariationDimensions.Any(d => string.Equals(d.Name, _name, StringComparison.OrdinalIgnoreCase)))
        {
            yield return $"dimension '{_name}' does not exist";
        }
    }
}
