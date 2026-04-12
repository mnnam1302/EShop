using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Update;

public sealed class ProductCanUpdateSpecification : Specification<ProductAggregate>
{
    private ProductCanUpdateSpecification()
    {
    }

    public static ProductCanUpdateSpecification New() => new ProductCanUpdateSpecification();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.Update))
        {
            yield return $"product {product.Id} in state '{product.State}' cannot be updated";
        }
    }
}
