namespace EShop.Tenancy.Presentation.Models;

public sealed class CreateSystemFeatureRequest
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string State { get; init; }
    public required string Module { get; init; }
}