using EShop.Catalog.SyncService.MongoDb.Infrastructure.Attributes;
using EShop.Shared.EventBus;

namespace EShop.Catalog.SyncService.MongoDb.Models;

[MongoCollection("InboxMessage")]
public class InboxMessageProjection : Document
{
    public string ConsumerId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string State { get; set; } = InboxMessageStatus.Pending;
    public string ReasonFailed { get; set; } = string.Empty;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    internal static InboxMessageProjection Create(string consumerId, Guid messageId, string messageType)
    {
        return new InboxMessageProjection
        {
            ConsumerId = consumerId,
            DocumentId = messageId,
            MessageType = messageType,
            State = InboxMessageStatus.Pending,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            UpdatedOnUtc = DateTimeOffset.UtcNow
        };
    }

    internal void MarkAsDone()
    {
        State = InboxMessageStatus.Done;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    internal void MarkAsFailed(string message)
    {
        State = InboxMessageStatus.Failed;
        ReasonFailed = message;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }
}