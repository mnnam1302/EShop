namespace EShop.Tenancy.Application.UseCases.Features.GetFeature;

public sealed class FeatureResponse
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string State { get; init; }
    public required string Module { get; init; }
    public required string DefaultStateForNewTenant { get; init; }
    public string? Category { get; init; }
}
