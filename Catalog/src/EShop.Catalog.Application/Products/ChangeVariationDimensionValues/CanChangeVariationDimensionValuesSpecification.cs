using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.ChangeVariationDimensionValues;

public sealed class CanChangeVariationDimensionValuesSpecification : Specification<ProductAggregate>
{
    private readonly string _dimensionName;
    private readonly string[] _newValues;

    private CanChangeVariationDimensionValuesSpecification(string dimensionName, string[] newValues)
    {
        _dimensionName = dimensionName;
        _newValues = newValues;
    }

    public static CanChangeVariationDimensionValuesSpecification New(string dimensionName, string[] newValues)
        => new(dimensionName, newValues);

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.ChangeVariationDimensionValues))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot change variation dimension values";
            yield break;
        }

        var dimension = product.VariationDimensions.FirstOrDefault(d =>
            string.Equals(d.Name, _dimensionName, StringComparison.OrdinalIgnoreCase));

        if (dimension is null)
        {
            yield return $"dimension '{_dimensionName}' does not exist";
            yield break;
        }

        if (_newValues.Length == 0)
        {
            yield return "at least one value is required";
        }
        else if (_newValues.Distinct(StringComparer.OrdinalIgnoreCase).Count() != _newValues.Length)
        {
            yield return "dimension values must be unique";
        }

        var removedValues = dimension.Values
            .Where(oldVal => !_newValues.Contains(oldVal, StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var removedValue in removedValues)
        {
            var isReferenced = product.Variants
                .Where(v => v.State != VariantState.Deleted)
                .Any(v => v.VariantDimensionValues
                    .Any(dv => string.Equals(dv.Name, _dimensionName, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(dv.Value, removedValue, StringComparison.OrdinalIgnoreCase)));

            if (isReferenced)
            {
                yield return $"'{removedValue}' cannot be removed because it is referenced by a variant";
            }
        }
    }
}
