using EShop.Shared.DomainTools.Specifications;

namespace EShop.Catalog.Application.Products.Publish;

public sealed class ProductCanPublishSpecification : Specification<ProductAggregate>
{
    private ProductCanPublishSpecification()
    {
    }

    public static ProductCanPublishSpecification New() => new();

    protected override IEnumerable<string> IsNotSatisfiedBecause(ProductAggregate product)
    {
        if (!product.State.CanFire(ProductAction.Publish))
        {
            yield return $"product {product.Id} in state '{product.State.State}' cannot be published";
        }

        if (product.Variants.Count == 0)
        {
            yield return "at least one variant is required";
        }
        else if (!product.Variants.Any(v => v.Price > 0))
        {
            yield return "at least one variant must have a price greater than zero";
        }

        if (string.IsNullOrEmpty(product.Name))
        {
            yield return "Name is required";
        }

        if (string.IsNullOrEmpty(product.Slug))
        {
            yield return "Slug is required";
        }

        if (product.CategoryId == Guid.Empty)
        {
            yield return "CategoryId is required";
        }
    }
}
