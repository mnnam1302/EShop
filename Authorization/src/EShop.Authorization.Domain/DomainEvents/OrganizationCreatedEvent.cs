namespace EShop.Authorization.Domain.DomainEvents;

public sealed class OrganizationCreatedEvent : OrganizationDomainEvent
{
    public required string Name { get; init; }
    public string? ParentOrganizationId { get; init; }
    public required string TenantId { get; init; }
    public required string Scope { get; init; }
}
