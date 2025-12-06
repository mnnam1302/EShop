using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.EventBus;

public class InboxMessage : IExcludedFromScoping
{
    public Guid Id { get; set; }

    [MaxLength(ModelConstants.ShortMediumText)]
    public string ConsumerId { get; set; } = string.Empty;

    public Guid MessageId { get; set; }

    [MaxLength(ModelConstants.ShortMediumText)]
    public string MessageType { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string State { get; set; } = InboxMessageStatus.Pending;

    [MaxLength(ModelConstants.VeryLongText)]
    public string ReasonFailed { get; set; } = string.Empty;

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }

    public static InboxMessage Create(string consumerId, Guid messageId, string messageType)
    {
        return new InboxMessage
        {
            Id = Guid.NewGuid(),
            ConsumerId = consumerId,
            MessageId = messageId,
            MessageType = messageType,
            State = InboxMessageStatus.Pending,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            UpdatedOnUtc = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsDone()
    {
        State = InboxMessageStatus.Done;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string failureReason)
    {
        State = InboxMessageStatus.Failed;
        ReasonFailed = failureReason;
        UpdatedOnUtc = DateTimeOffset.UtcNow;
    }
}

public static class InboxMessageStatus
{
    public const string Pending = "Pending";
    public const string Done = "Done";
    public const string Failed = "Failed";
}