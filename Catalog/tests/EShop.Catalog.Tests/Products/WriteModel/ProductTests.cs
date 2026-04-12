using EShop.Catalog.Application.Products;
using EShop.Catalog.Application.Products.Create;
using EShop.Catalog.Application.Products.Update;
using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Catalog.Tests.Products.WriteModel;

public class ProductTests
{
    [Fact]
    public void Create_ValidCommand_SetsPropertiesAndInitialState()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test description",
            CategoryId = categoryId,
            Tags = ["tag1"],
            Slug = "test-product",
            Images = ["img1.jpg"],
            Groups = []
        };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider("user-1", "tenant-1");

        // Act
        var product = ProductAggregate.Create(command, userDetails);

        // Assert
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal("test-product", product.Slug);
        Assert.Equal(ProductState.Draft, product.State.State);
        Assert.Equal("tenant-1", product.TenantId);
        Assert.Equal("user-1", product.CreatedByUserId);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Published")]
    [InlineData("Unpublished")]
    public void Update_ActiveProduct_ChangesProperties(string state)
    {
        // Arrange
        var product = CreateProductInState(state);
        var newCategoryId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "Updated",
            Description = "Updated",
            CategoryId = newCategoryId,
            Tags = ["new"],
            Slug = "updated",
            Images = ["new.jpg"],
            Groups = []
        };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.Update(command, userDetails);

        // Assert
        Assert.Equal("Updated", product.Name);
        Assert.Equal(newCategoryId, product.CategoryId);
    }

    [Fact]
    public void Update_DeletedProduct_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateDeletedProduct();
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "X",
            Description = "",
            CategoryId = Guid.NewGuid(),
            Tags = [],
            Slug = "x",
            Images = [],
            Groups = []
        };
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.Update(command, userDetails));
    }

    [Fact]
    public void Publish_DraftWithVariant_TransitionsToPublished()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateDraftProduct();
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.Publish(userDetails);

        // Assert
        Assert.Equal(ProductState.Published, product.State.State);
        Assert.NotNull(product.LastModifiedAtUtc);
    }

    [Fact]
    public void Publish_WithoutVariants_Throws()
    {
        // Arrange
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

        // Act & Assert
        Assert.Throws<DomainException>(() => product.Publish(userDetails));
    }

    [Fact]
    public void Publish_AlreadyPublished_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreatePublishedProduct();
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.Publish(userDetails));
    }

    [Fact]
    public void Unpublish_PublishedProduct_TransitionsToUnpublished()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreatePublishedProduct();
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.Unpublish(userDetails);

        // Assert
        Assert.Equal(ProductState.Unpublished, product.State.State);
    }

    [Fact]
    public void Unpublish_DraftProduct_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreateDraftProduct();
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.Unpublish(userDetails));
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Unpublished")]
    public void Delete_NonPublishedProduct_TransitionsToDeleted(string state)
    {
        // Arrange
        var product = CreateProductInState(state);
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act
        product.Delete(userDetails);

        // Assert
        Assert.Equal(ProductState.Deleted, product.State.State);
    }

    [Fact]
    public void Delete_PublishedProduct_Throws()
    {
        // Arrange
        var product = ProductAggregateBuilder.CreatePublishedProduct();
        var userDetails = ProductAggregateBuilder.CreateUserDetailsProvider();

        // Act & Assert
        Assert.Throws<DomainException>(() => product.Delete(userDetails));
    }

    private static ProductAggregate CreateProductInState(string state) => state switch
    {
        "Draft" => ProductAggregateBuilder.CreateDraftProduct(),
        "Published" => ProductAggregateBuilder.CreatePublishedProduct(),
        "Unpublished" => ProductAggregateBuilder.CreateUnpublishedProduct(),
        _ => throw new ArgumentException($"Unknown state: {state}")
    };
}