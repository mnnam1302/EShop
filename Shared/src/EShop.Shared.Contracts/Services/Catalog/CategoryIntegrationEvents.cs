namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class CategoryCreated : CatalogIntegrationEvent
{
    public required Guid CategoryId { get; init; }
    public required ulong Version { get; init; }
    public required string Name { get; init; }
    public required string Reference { get; init; }
    public required string Slug { get; init; }
    public Guid? ParentId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset UpdatedAtUtc { get; init; }
}
