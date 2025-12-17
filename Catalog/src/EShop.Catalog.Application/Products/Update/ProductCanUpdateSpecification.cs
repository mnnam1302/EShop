using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Update;

public sealed class ProductCanUpdateSpecification : Specification<ProductAggregate>
{
    private ProductCanUpdateSpecification()
    {
    }

    public static ProductCanUpdateSpecification New() => new ProductCanUpdateSpecification();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (!obj.State.CanFire(ProductAction.Update))
        {
            yield return $"product {obj.Id} in state '{obj.State}' cannot be updated";
        }
    }
}
