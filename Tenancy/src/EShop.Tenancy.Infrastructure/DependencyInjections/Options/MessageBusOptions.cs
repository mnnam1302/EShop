using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Infrastructure.DependencyInjections.Options;

public record MessageBusOptions
{
    public int RetryLimit { get; init; }

    [Required, Timestamp]
    public TimeSpan InitialInterval { get; init; }

    [Required, Timestamp]
    public TimeSpan IntervalIncrement { get; init; }
}