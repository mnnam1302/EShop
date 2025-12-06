namespace EShop.Shared.EventBus.Abstractions;

public interface IMessageRepository
{
    Task<bool> ExistsAsync(Guid messageId, string consumerId, CancellationToken cancellationToken);

    Task AddAsync(InboxMessage inboxMessage, CancellationToken cancellationToken);
}