namespace EShop.Shared.ReadModel;

/// <summary>
/// Marker interface for read models that receive projected events.
/// </summary>
public interface IReadModel
{
    /// <summary>
    /// Unique identifier of the read model instance.
    /// </summary>
    string Id { get; set; }
}
