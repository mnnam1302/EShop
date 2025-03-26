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

    public DateTimeOffset CreatedOnUtc { get; set; }
}