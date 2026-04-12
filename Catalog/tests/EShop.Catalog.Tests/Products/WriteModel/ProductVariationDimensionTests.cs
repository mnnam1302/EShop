using EShop.Catalog.Application.Products;
using EShop.Catalog.Application.Products.AddVariationDimension;
using EShop.Catalog.Application.Products.ChangeVariationDimensionValues;
using EShop.Catalog.Application.Products.UpdateVariationDimension;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Catalog.Tests.Products.WriteModel;

public class ProductVariationDimensionTests
{
    [Fact]
    public void AddVariationDimension_ValidCommand_AddsDimension()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateDraftProduct();
        var command = new AddVariationDimensionCommand
        {
            ProductId = product.Id,
            Name = "Color",
            DisplayName = "Color",
            Values = ["Red", "Blue"],
            DisplayStyle = "Text"
        };

        // Act
        product.AddVariationDimension(command);

        // Assert
        var dimension = Assert.Single(product.VariationDimensions);
        Assert.Equal("Color", dimension.Name);
        Assert.Equal(["Red", "Blue"], dimension.Values);
        Assert.Equal(VariationDisplayStyles.Text, dimension.DisplayStyle);
    }

    [Fact]
    public void AddVariationDimension_DuplicateNameCaseInsensitive_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithDimensions(("Color", ["Red"]));
        var command = new AddVariationDimensionCommand
        {
            ProductId = product.Id,
            Name = "color",
            DisplayName = "C",
            Values = ["Green"],
            DisplayStyle = "Text"
        };

        // Act & Assert
        Assert.Throws<DomainException>(() => product.AddVariationDimension(command));
    }

    [Fact]
    public void AddVariationDimension_WhenNonDefaultVariantsExist_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithVariant(out _);
        var command = new AddVariationDimensionCommand
        {
            ProductId = product.Id,
            Name = "Size",
            DisplayName = "Size",
            Values = ["S", "M"],
            DisplayStyle = "Text"
        };

        // Act & Assert
        Assert.Throws<DomainException>(() => product.AddVariationDimension(command));
    }

    [Fact]
    public void UpdateVariationDimension_ExistingDimension_UpdatesDisplayFields()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithDimensions(("Color", ["Red"]));
        var command = new UpdateVariationDimensionCommand
        {
            ProductId = product.Id,
            Name = "Color",
            DisplayName = "Colour",
            DisplayStyle = "Color"
        };

        // Act
        product.UpdateVariationDimension(command);

        // Assert
        var dimension = product.VariationDimensions.First(d => d.Name == "Color");
        Assert.Equal("Colour", dimension.DisplayName);
        Assert.Equal(VariationDisplayStyles.Color, dimension.DisplayStyle);
    }

    [Fact]
    public void ChangeVariationDimensionValues_ValidValues_ReplacesValues()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithDimensions(("Color", ["Red", "Blue"]));
        var command = new ChangeVariationDimensionValuesCommand
        {
            ProductId = product.Id,
            DimensionName = "Color",
            Values = ["Red", "Green", "Yellow"]
        };

        // Act
        product.ChangeVariationDimensionValues(command);

        // Assert
        var dimension = product.VariationDimensions.First(d => d.Name == "Color");
        Assert.Equal(["Red", "Green", "Yellow"], dimension.Values);
    }

    [Fact]
    public void ChangeVariationDimensionValues_RemovesReferencedValue_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateProductWithVariant(out _);
        var command = new ChangeVariationDimensionValuesCommand
        {
            ProductId = product.Id,
            DimensionName = "Color",
            Values = ["Blue"]
        };

        // Act & Assert
        Assert.Throws<DomainException>(() => product.ChangeVariationDimensionValues(command));
    }
}
