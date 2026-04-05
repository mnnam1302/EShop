using EShop.Catalog.Application.Products;
using EShop.Catalog.Application.Products.AddVariant;
using EShop.Catalog.Application.Products.AddVariationDimension;
using EShop.Catalog.Application.Products.Create;
using EShop.Catalog.Application.Products.PublishVariant;
using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using Moq;

namespace EShop.Catalog.Tests.Products;

internal static class ProductAggregateBuilder
{
    private static readonly IUserDetailsProvider DefaultUserDetailsProvider = CreateUserDetailsProvider();

    public static IUserDetailsProvider CreateUserDetailsProvider(
        string userId = "user-1",
        string tenantId = "tenant-1")
    {
        var mock = new Mock<IUserDetailsProvider>();
        mock.Setup(x => x.AuthenticatedUser)
            .Returns(new UserData(userId, "testuser", tenantId));
        return mock.Object;
    }

    public static ProductAggregate CreateDraftProduct(
        string name = "Test Product",
        string slug = "test-product",
        Guid? categoryId = null,
        decimal price = 100m,
        decimal discountPrice = 0m)
    {
        var command = new CreateProductCommand
        {
            Name = name,
            Description = "Test description",
            CategoryId = categoryId ?? Guid.NewGuid(),
            Tags = ["tag1"],
            Slug = slug,
            Images = ["image1.jpg"],
            Groups = []
        };

        var product = ProductAggregate.Create(command, DefaultUserDetailsProvider);
        product.AddDefaultVariant(price, discountPrice);
        product.MarkEventsAsCommitted();
        return product;
    }

    public static ProductAggregate CreatePublishedProduct(
        string name = "Test Product",
        string slug = "test-product",
        decimal price = 100m)
    {
        var product = CreateDraftProduct(name, slug, price: price);
        product.Publish(DefaultUserDetailsProvider);
        product.MarkEventsAsCommitted();
        return product;
    }

    public static ProductAggregate CreateUnpublishedProduct(
        string name = "Test Product",
        string slug = "test-product",
        decimal price = 100m)
    {
        var product = CreatePublishedProduct(name, slug, price);
        product.Unpublish(DefaultUserDetailsProvider);
        product.MarkEventsAsCommitted();
        return product;
    }

    public static ProductAggregate CreateDeletedProduct()
    {
        var product = CreateDraftProduct();
        product.Delete(DefaultUserDetailsProvider);
        product.MarkEventsAsCommitted();
        return product;
    }

    public static ProductAggregate CreateProductWithDimensions(
        params (string Name, string[] Values)[] dimensions)
    {
        var product = CreateDraftProduct();
        foreach (var (name, values) in dimensions)
        {
            product.AddVariationDimension(new AddVariationDimensionCommand
            {
                ProductId = product.Id,
                Name = name,
                DisplayName = name,
                Values = values,
                DisplayStyle = "Text"
            });
        }
        product.MarkEventsAsCommitted();
        return product;
    }

    public static ProductAggregate CreateProductWithVariant(
        out Guid variantId,
        string sku = "SKU-001",
        decimal price = 100m)
    {
        var product = CreateProductWithDimensions(("Color", ["Red", "Blue"]));
        product.AddVariant(new AddVariantCommand
        {
            ProductId = product.Id,
            Name = "Red Variant",
            Sku = sku,
            Price = price,
            DiscountPrice = 0m,
            DimensionValues = [new VariantDimensionValueInput { Name = "Color", Value = "Red" }]
        });
        variantId = product.Variants.Last(v => !v.IsDefault).Id;
        product.MarkEventsAsCommitted();
        return product;
    }

    public static ProductAggregate CreateProductWithPublishedVariant(
        out Guid variantId,
        string sku = "SKU-001",
        decimal price = 100m)
    {
        var product = CreateProductWithVariant(out variantId, sku, price);
        product.PublishVariant(
            new PublishVariantCommand { ProductId = product.Id, VariantId = variantId },
            DefaultUserDetailsProvider);
        product.MarkEventsAsCommitted();
        return product;
    }
}