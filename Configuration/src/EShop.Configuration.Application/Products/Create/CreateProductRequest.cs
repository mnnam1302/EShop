namespace EShop.Configuration.Application.Products.Create;

/// <summary>
/// Request to create a new product
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// The name of the product
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The description of the product
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// The price of the product
    /// </summary>
    public decimal Price { get; init; }
}
