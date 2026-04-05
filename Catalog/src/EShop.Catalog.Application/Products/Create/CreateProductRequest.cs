namespace EShop.Catalog.Application.Products.Create;

public sealed class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }
    public IEnumerable<string> Tags { get; set; } = [];
    public string Slug { get; set; } = string.Empty;
    public IEnumerable<string> Images { get; set; } = [];
    public IEnumerable<Guid> Groups { get; set; } = [];
}
