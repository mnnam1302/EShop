namespace EShop.Shared.ReadModel;

/// <summary>
/// Default implementation of <see cref="IReadModelContext"/>.
/// </summary>
public sealed class ReadModelContext : IReadModelContext
{
    public ReadModelContext(bool isNew)
    {
        IsNew = isNew;
    }

    public bool IsNew { get; }

    public bool IsMarkedForDeletion { get; private set; }

    public void MarkForDeletion() => IsMarkedForDeletion = true;
}
