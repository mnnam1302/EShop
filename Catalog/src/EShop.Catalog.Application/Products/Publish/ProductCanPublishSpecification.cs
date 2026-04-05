using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Publish;

public sealed class ProductCanPublishSpecification : Specification<ProductAggregate>
{
    private ProductCanPublishSpecification()
    {
    }

    public static ProductCanPublishSpecification New() => new();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate obj)
    {
        if (!obj.State.CanFire(ProductAction.Publish))
        {
            yield return $"product {obj.Id} in state '{obj.State.State}' cannot be published";
        }

        if (obj.Variants.Count == 0)
        {
            yield return "at least one variant is required";
        }
        else if (!obj.Variants.Any(v => v.Price > 0))
        {
            yield return "at least one variant must have a price greater than zero";
        }

        if (string.IsNullOrEmpty(obj.Name))
        {
            yield return "Name is required";
        }

        if (string.IsNullOrEmpty(obj.Slug))
        {
            yield return "Slug is required";
        }

        if (obj.CategoryId == Guid.Empty)
        {
            yield return "CategoryId is required";
        }
    }
}
