using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Create;

public sealed class ProductCanAddVariantSpecification : Specification<ProductAggregate>
{
    private readonly bool _isDefault;
    private readonly IReadOnlyList<VariantDimensionValue> _variantDimensionValues;

    private ProductCanAddVariantSpecification(bool isDefault, IReadOnlyList<VariantDimensionValue> values)
    {
        _isDefault = isDefault;
        _variantDimensionValues = values;
    }

    public static ProductCanAddVariantSpecification New(bool isDefault, IReadOnlyList<VariantDimensionValue> values)
    {
        return new ProductCanAddVariantSpecification(isDefault, values);
    }

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (_isDefault)
        {
            if (_variantDimensionValues.Any())
                yield return "default variant must not have dimension values";

            if (obj.Variants.Any(v => v.IsDefault))
                yield return "default variant already exists";

            yield break;
        }

        if (obj.VariationDimensions.Count != _variantDimensionValues.Count)
        {
            yield return "Too many or not enough values provided to dimensions";
            yield break;
        }

        foreach (var dimension in obj.VariationDimensions)
        {
            if (!_variantDimensionValues.Any(v => v.Name == dimension.Name))
            {
                yield return $"Dimension value not provided for {dimension.Name}";
            }
        }
    }
}