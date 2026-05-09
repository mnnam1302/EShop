using EShop.Catalog.Application.Products;
using EShop.Catalog.Application.Products.AddVariant;
using EShop.Catalog.Application.Products.ChangeVariantPrice;
using EShop.Catalog.Application.Products.Create;
using EShop.Catalog.Application.Products.PublishVariant;
using EShop.Catalog.Application.Products.UnpublishVariant;
using EShop.Catalog.Application.Products.UpdateVariant;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Catalog.Tests.Products.WriteModel;

public class ProductVariantTests
{
    [Fact]
    public void AddVariant_ValidDimensions_AddsNonDefaultVariant()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithDimensions(("Color", ["Red", "Blue"]));
        var command = new AddVariantCommand
        {
            ProductId = product.Id,
            Name = "Red Variant",
            Sku = "SKU-RED",
            Price = 200m,
            DiscountPrice = 20m,
            DimensionValues = [new VariantDimensionValueInput { Name = "Color", Value = "Red" }]
        };

        // Act
        product.AddVariant(command);

        // Assert
        var variant = product.Variants.Last(v => !v.IsDefault);
        Assert.False(variant.IsDefault);
        Assert.Equal("Red Variant", variant.Name);
        Assert.Equal("SKU-RED", variant.Sku);
        Assert.Equal(200m, variant.Price);
    }

    [Fact]
    public void AddVariant_InvalidDimensionValue_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithDimensions(("Color", ["Red", "Blue"]));
        var command = new AddVariantCommand
        {
            ProductId = product.Id,
            Name = "V",
            Sku = "S",
            Price = 100m,
            DiscountPrice = 0m,
            DimensionValues = [new VariantDimensionValueInput { Name = "Color", Value = "Green" }]
        };

        // Act & Assert
        Assert.Throws<DomainException>(() => product.AddVariant(command));
    }

    [Fact]
    public void AddVariant_DuplicateDimensionCombination_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithVariant(out _);
        var duplicate = new AddVariantCommand
        {
            ProductId = product.Id,
            Name = "Duplicate",
            Sku = "S2",
            Price = 100m,
            DiscountPrice = 0m,
            DimensionValues = [new VariantDimensionValueInput { Name = "Color", Value = "Red" }]
        };

        // Act & Assert
        Assert.Throws<DomainException>(() => product.AddVariant(duplicate));
    }

    [Fact]
    public void UpdateVariant_ExistingVariant_UpdatesNameAndSku()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithVariant(out var variantId);
        var command = new UpdateVariantCommand
        {
            ProductId = product.Id,
            VariantId = variantId,
            Name = "Updated",
            Sku = "NEW-SKU"
        };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.UpdateVariant(command, userDetails);

        // Assert
        var variant = product.Variants.First(v => v.Id == variantId);
        Assert.Equal("Updated", variant.Name);
        Assert.Equal("NEW-SKU", variant.Sku);
    }

    [Fact]
    public void ChangeVariantPrice_ValidAmount_UpdatesPrice()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithVariant(out var variantId);
        var command = new ChangeVariantPriceCommand
        {
            ProductId = product.Id,
            VariantId = variantId,
            Price = 200m,
            DiscountPrice = 50m
        };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.ChangeVariantPrice(command, userDetails);

        // Assert
        var variant = product.Variants.First(v => v.Id == variantId);
        Assert.Equal(200m, variant.Price);
        Assert.Equal(50m, variant.DiscountPrice);
    }

    [Fact]
    public void PublishVariant_ValidVariant_TransitionsToPublished()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithVariant(out var variantId);
        var command = new PublishVariantCommand { ProductId = product.Id, VariantId = variantId };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.PublishVariant(command, userDetails);

        // Assert
        var variant = product.Variants.First(v => v.Id == variantId);
        Assert.Equal(VariantState.Published, variant.State);
    }

    [Fact]
    public void UnpublishVariant_PublishedVariant_TransitionsToUnpublished()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithPublishedVariant(out var variantId);
        var command = new UnpublishVariantCommand { ProductId = product.Id, VariantId = variantId };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.UnpublishVariant(command, userDetails);

        // Assert
        var variant = product.Variants.First(v => v.Id == variantId);
        Assert.Equal(VariantState.Unpublished, variant.State);
    }

    [Fact]
    public void UnpublishVariant_LastPublishedOnPublishedProduct_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithPublishedVariant(out var variantId);
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();
        product.Publish(userDetails);
        product.MarkEventsAsCommitted();

        var command = new UnpublishVariantCommand { ProductId = product.Id, VariantId = variantId };

        // Act & Assert
        Assert.Throws<DomainException>(() => product.UnpublishVariant(command, userDetails));
    }

    private static ProductAggregate CreateBareProduct()
    {
        var command = new CreateProductCommand
        {
            Name = "P",
            Description = "",
            CategoryId = Guid.NewGuid(),
            Tags = [],
            Slug = "p",
            Images = [],
            Groups = []
        };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();
        var product = ProductAggregate.Create(command, userDetails);
        product.MarkEventsAsCommitted();
        return product;
    }
}
