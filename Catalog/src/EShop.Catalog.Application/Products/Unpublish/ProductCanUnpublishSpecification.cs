using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Unpublish;

public sealed class ProductCanUnpublishSpecification : Specification<ProductAggregate>
{
    private ProductCanUnpublishSpecification()
    {
    }

    public static ProductCanUnpublishSpecification New() => new();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.Unpublish))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot be unpublished";
        }
    }
}
