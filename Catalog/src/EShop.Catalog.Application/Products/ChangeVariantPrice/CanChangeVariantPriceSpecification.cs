using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.ChangeVariantPrice;

public sealed class CanChangeVariantPriceSpecification : Specification<ProductAggregate>
{
    private readonly Guid _variantId;
    private readonly decimal _price;
    private readonly decimal _discountPrice;

    private CanChangeVariantPriceSpecification(Guid variantId, decimal price, decimal discountPrice)
    {
        _variantId = variantId;
        _price = price;
        _discountPrice = discountPrice;
    }

    public static CanChangeVariantPriceSpecification New(Guid variantId, decimal price, decimal discountPrice)
        => new(variantId, price, discountPrice);

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (obj.State.State == ProductState.Deleted)
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

        if (_price <= 0)
        {
            yield return "Price must be greater than zero";
        }

        if (_discountPrice < 0)
        {
            yield return "DiscountPrice must be greater than or equal to zero";
        }

        if (_discountPrice > _price)
        {
            yield return "DiscountPrice must be less than or equal to Price";
        }
    }
}
