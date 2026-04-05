namespace EShop.Shared.ReadModel;

/// <summary>
/// Resolves the read model identity from an incoming event.
/// Each read model type has its own locator instance.
/// </summary>
/// <typeparam name="TReadModel">The read model type this locator serves.</typeparam>
public interface IReadModelLocator<TReadModel> where TReadModel : class, IReadModel
{
    /// <summary>
    /// Extracts the read model identifier from the given event.
    /// </summary>
    /// <param name="event">The incoming event.</param>
    /// <returns>The read model identifier as a string.</returns>
    string GetReadModelId(object @event);
}
