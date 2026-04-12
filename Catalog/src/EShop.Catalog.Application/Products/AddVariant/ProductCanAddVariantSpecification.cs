using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.AddVariant;

public sealed class ProductCanAddVariantSpecification : Specification<ProductAggregate>
{
    private readonly bool _isDefault;
    private readonly IReadOnlyList<VariantDimensionValue> _variantDimensionValues;

    private ProductCanAddVariantSpecification(bool isDefault, IReadOnlyList<VariantDimensionValue> variantDimensionValues)
    {
        _isDefault = isDefault;
        _variantDimensionValues = variantDimensionValues;
    }

    public static ProductCanAddVariantSpecification New(bool isDefault, IReadOnlyList<VariantDimensionValue> variantDimensionValues)
    {
        return new ProductCanAddVariantSpecification(isDefault, variantDimensionValues);
    }

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.AddVariant))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot add variant";
            yield break;
        }

        if (_isDefault)
        {
            if (_variantDimensionValues.Any())
            {
                yield return "default variant must not have dimension values";
            }

            if (product.Variants.Any(variant => variant.IsDefault))
            {
                yield return "default variant already exists";
            }

            yield break;
        }

        if (product.VariationDimensions.Count != _variantDimensionValues.Count)
        {
            yield return "Too many or not enough values provided to dimensions";
            yield break;
        }

        foreach (var dimension in product.VariationDimensions)
        {
            var candidateValue = _variantDimensionValues.FirstOrDefault(candidate => candidate.Name == dimension.Name);

            if (candidateValue is null)
            {
                yield return $"Dimension value not provided for {dimension.Name}";
                continue;
            }

            if (!dimension.Values.Contains(candidateValue.Value, StringComparer.OrdinalIgnoreCase))
            {
                yield return $"'{candidateValue.Value}' is not a valid value for dimension '{dimension.Name}'";
            }
        }

        var candidateKey = BuildDimensionKey(_variantDimensionValues);

        var hasDuplicate = product.Variants
            .Where(variant => !variant.IsDefault && variant.State != VariantState.Deleted)
            .Any(variant => string.Equals(BuildDimensionKey(variant.VariantDimensionValues), candidateKey, StringComparison.OrdinalIgnoreCase));

        if (hasDuplicate)
        {
            yield return "a variant with this dimension value combination already exists";
        }
    }

    private static string BuildDimensionKey(IReadOnlyList<VariantDimensionValue> dimensionValues)
    {
        return string.Join("|", dimensionValues
            .OrderBy(dv => dv.Name, StringComparer.OrdinalIgnoreCase)
            .Select(dv => $"{dv.Name}:{dv.Value}"));
    }
}