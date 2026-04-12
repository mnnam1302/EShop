namespace EShop.Shared.ReadModel;

/// <summary>
/// Context passed to read model <see cref="IAmReadModelFor{TEvent}.Apply"/> methods,
/// providing metadata about the current projection operation.
/// </summary>
public interface IReadModelContext
{
    /// <summary>
    /// Whether this is a newly created read model instance (not yet persisted).
    /// </summary>
    bool IsNew { get; }

    /// <summary>
    /// Marks the read model for deletion after the Apply method completes.
    /// </summary>
    void MarkForDeletion();

    /// <summary>
    /// Whether <see cref="MarkForDeletion"/> has been called during the current projection.
    /// </summary>
    bool IsMarkedForDeletion { get; }
}
