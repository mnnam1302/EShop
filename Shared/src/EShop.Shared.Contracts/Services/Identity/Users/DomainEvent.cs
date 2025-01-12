using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Identity.Users;

public static class DomainEvent
{
    public record UserCreated : IDomainEvent
    {
        public Guid EventId { get; init; }
        public DateTimeOffset TimeStamp { get; init; }
        public string SourceId { get; init; }
        public string Username { get; init; }
        public string Email { get; init; }
        public string? DisplayName { get; init; }
        public string? PhoneNumber { get; init; }
        public string OrganizationId { get; init; }
    }
}