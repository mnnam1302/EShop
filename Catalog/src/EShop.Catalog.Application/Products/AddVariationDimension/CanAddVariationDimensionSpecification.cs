using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.AddVariationDimension;

public sealed class CanAddVariationDimensionSpecification : Specification<ProductAggregate>
{
    private readonly string _name;
    private readonly string[] _values;

    private CanAddVariationDimensionSpecification(string name, string[] values)
    {
        _name = name;
        _values = values;
    }

    public static CanAddVariationDimensionSpecification New(string name, string[] values) => new(name, values);

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.AddVariationDimension))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot add variation dimension";
            yield break;
        }

        if (product.VariationDimensions.Any(d => string.Equals(d.Name, _name, StringComparison.OrdinalIgnoreCase)))
        {
            yield return $"a dimension with name '{_name}' already exists";
        }

        if (_values.Length == 0)
        {
            yield return "at least one value is required";
        }
        else if (_values.Distinct(StringComparer.OrdinalIgnoreCase).Count() != _values.Length)
        {
            yield return "dimension values must be unique";
        }

        if (product.Variants.Any(v => !v.IsDefault && v.State != VariantState.Deleted))
        {
            yield return "dimensions cannot be added when non-default variants exist";
        }
    }
}
