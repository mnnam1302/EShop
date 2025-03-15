using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.EventBus.DependencyInjections.Options;

public class MessageBusOptions
{
    public int RetryLimit { get; init; }

    [Required, Timestamp]
    public TimeSpan InitialInterval { get; init; }

    [Required, Timestamp]
    public TimeSpan IntervalIncrement { get; init; }
}