using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Authorization.Domain.DomainEvents;

public sealed class UserCreatedDomainEvent : IDomainEvent
{
    public DateTimeOffset TimeStamp => DateTimeOffset.UtcNow;

    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string RawPassword { get; init; }
}
