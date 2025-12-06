using System.ComponentModel.DataAnnotations;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class EventStoreOptions
{
    public bool IncludeSnapshots { get; set; } = false;

    [Required, Range(3, 100)]
    public int SnapshotIntervalInEvents { get; set; } = 3;
}
