namespace EShop.Tenancy.Application.UseCases.Tenants.GetTenant;

public sealed class TenantDetailsResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string OwnerUsername { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Description { get; init; }
}
