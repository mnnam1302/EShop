namespace EShop.Shared.ReadModel;

/// <summary>
/// Orchestrates the read model projection flow: locate → load → apply → persist.
/// </summary>
/// <typeparam name="TReadModel">The read model type to project onto.</typeparam>
public interface IReadModelProjector<TReadModel> where TReadModel : class, IReadModel
{
    /// <summary>
    /// Projects an event onto the read model.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="event">The event to project.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ProjectAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
}
