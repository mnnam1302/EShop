namespace EShop.Shared.Sequences.DependencyInjections;

public class SequenceManagerOptions
{
    /// <summary>
    /// Configures how many sequence values are reserved in a single call to persistant store.
    /// Higher values increase performance by reducing calls to persistant store but
    /// may create bigger gaps in sequences in case the host is restarted and 
    /// the in-memory sequence tracking is re-loaded from the persistant store.
    /// </summary>
    public int ReservedSequenceValueRange { get; set; } = 10;
}