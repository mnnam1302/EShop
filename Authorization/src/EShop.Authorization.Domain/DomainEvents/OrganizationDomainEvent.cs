namespace EShop.Authorization.Domain.DomainEvents;

public abstract class OrganizationDomainEvent : IAuthorizationDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTimeOffset TimeStampUtc { get; set; } = DateTimeOffset.UtcNow;
    public ulong Version { get; set; }
    public required string OrganizationId { get; set; }
}
