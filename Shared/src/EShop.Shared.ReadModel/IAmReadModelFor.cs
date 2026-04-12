namespace EShop.Shared.ReadModel;

/// <summary>
/// Declares that a read model can apply a specific event type.
/// </summary>
/// <typeparam name="TEvent">The event type this read model can project.</typeparam>
public interface IAmReadModelFor<in TEvent>
{
    /// <summary>
    /// Applies the event to the read model, updating its state.
    /// </summary>
    /// <param name="event">The event to apply.</param>
    /// <param name="context">Context providing metadata about the projection operation.</param>
    void Apply(TEvent @event, IReadModelContext context);
}
