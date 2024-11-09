using System.ComponentModel.DataAnnotations;

namespace Identity.Persistence.DependencuInjections.Options;

public class NgSqlRetryOptions
{
    [Required, Range(5, 20)]
    public int MaxRetryCount { get; init; }

    [Required, Timestamp]
    public TimeSpan MaxRetryDelay { get; init; }

    public string[]? ErrorNumbersoAdd { get; init; }
}