using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Unpublish;

public sealed class ProductCanUnpublishSpecification : Specification<ProductAggregate>
{
    private ProductCanUnpublishSpecification()
    {
    }

    public static ProductCanUnpublishSpecification New() => new();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (!obj.State.CanFire(ProductAction.Unpublish))
        {
            yield return $"product {obj.Id} in state '{obj.State.State}' cannot be unpublished";
        }
    }
}
