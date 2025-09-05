namespace EShop.Configuration.Application.Agencies.GetAgencies;

public sealed class GetAgenciesResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string TenantId { get; init; }
}
