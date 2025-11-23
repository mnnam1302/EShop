using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Authorization.Domain.DomainEvents;

public static class OrganizationEvents
{
    public sealed record RootOrganizationCreated : IDomainEvent
    {
        public Guid EventId { get; set; }
        public ulong Version { get; set; }
        public DateTimeOffset TimeStampUtc { get; set; }

        public required string OrganizationId { get; init; }
        public required string Name { get; init; }
        public required string TenantId { get; init; }
    }
}