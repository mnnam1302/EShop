using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Persistence.DependencyInjections.Options;

public class NgSqlRetryOptions
{
    [Required, Range(5, 20)]
    public int MaxRetryCount { get; init; }

    [Required, Timestamp]
    public TimeSpan MaxRetryDelay { get; init; }

    public string[]? ErrorNumbersoAdd { get; init; }
}