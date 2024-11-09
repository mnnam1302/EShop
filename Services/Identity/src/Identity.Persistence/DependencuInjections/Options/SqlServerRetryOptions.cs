using System.ComponentModel.DataAnnotations;

namespace Identity.Persistence.DependencuInjections.Options;

public class SqlServerRetryOptions
{
    [Required, Range(5, 20)]
    public int MaxRetryCount { get; init; }

    [Required, Timestamp]
    public TimeSpan MaxRetryDelay { get; init; }

    public int[]? ErrorNumbersoAdd { get; init; }
}