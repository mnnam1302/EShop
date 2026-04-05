using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Delete;

public sealed class ProductCanDeleteSpecification : Specification<ProductAggregate>
{
    private ProductCanDeleteSpecification()
    {
    }

    public static ProductCanDeleteSpecification New() => new();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.Delete))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot be deleted";
        }
    }
}