namespace EShop.Configuration.Application.Products.Create;

public sealed class CreateProductRequest
{
    public required string Name { get; init; }

    public string? AgencyId { get; init; }
}
