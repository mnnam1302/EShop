using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Authorization.Domain.DomainEvents;

public static class OrganizationEvents
{
    public sealed record RootOrganizationCreated : IDomainEvent
    {
        public required Guid EventId { get; init; }
        public required DateTimeOffset TimeStamp { get; init; }
        public required string OrganizationId { get; init; }
        public required string Name { get; init; }
        public required string TenantId { get; init; }
    }
}