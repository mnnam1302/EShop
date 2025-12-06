namespace EShop.Shared.Contracts.Services.Authorization;

public sealed class OrganizationCreated : AuthorizationIntegrationEvent
{
    public required string OrganizationId { get; init; }
    public required string Name { get; init; }
    public string? ParentOrganizationId { get; init; }
}
