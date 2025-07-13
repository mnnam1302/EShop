using EShop.Shared.Contracts.Shared;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.EventBus;

public class InboxMessage : IExcludedFromScoping
{
    public Guid MessageId { get; set; }

    [MaxLength(ModelConstants.ShortMediumText)]
    public string MessageType { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortMediumText)]
    public string ConsumerId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string State { get; set; } = InboxMessageStatus.New.ToString();

    [MaxLength(ModelConstants.VeryLongText)]
    public string ReasonFailed { get; set; } = string.Empty;

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset UpdatedOnUtc { get; set; }
}

public enum InboxMessageStatus
{
    New,
    Done,
    Failed
}