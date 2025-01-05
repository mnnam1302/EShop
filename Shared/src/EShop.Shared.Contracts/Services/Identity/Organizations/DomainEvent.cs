using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Identity.Organizations;

public static class DomainEvent
{
    public record OrganizationCreated : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTimeOffset TimeStamp { get; init; }

        public string SourceId { get; init; }
        public string Name { get; init; }
        public string? OrganizationNumber { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Email { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
    }
}