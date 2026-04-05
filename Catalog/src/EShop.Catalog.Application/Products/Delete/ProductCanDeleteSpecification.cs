using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Delete;

public sealed class ProductCanDeleteSpecification : Specification<ProductAggregate>
{
    private ProductCanDeleteSpecification()
    {
    }

    public static ProductCanDeleteSpecification New() => new();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (!obj.State.CanFire(ProductAction.Delete))
        {
            yield return $"product {obj.Id} in state '{obj.State.State}' cannot be deleted";
        }
    }
}