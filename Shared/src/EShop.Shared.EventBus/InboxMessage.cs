using EShop.Shared.Contracts.Shared;
using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.EventBus;

public class InboxMessage
{
    public Guid MessageId { get; set; }

    [MaxLength(ModelConstants.ShortMediumText)]
    public string MessageType { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortMediumText)]
    public string ConsumerId { get; set; } = string.Empty;

    public DateTimeOffset CreatedOnUtc { get; set; }
}