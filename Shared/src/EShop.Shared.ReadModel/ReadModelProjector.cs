namespace EShop.Shared.ReadModel;

public sealed class ReadModelProjector<TReadModel> : IReadModelProjector<TReadModel>
    where TReadModel : class, IReadModel, new()
{
    private readonly IReadModelStore<TReadModel> _store;
    private readonly IReadModelLocator<TReadModel> _locator;

    public ReadModelProjector(
        IReadModelStore<TReadModel> store,
        IReadModelLocator<TReadModel> locator)
    {
        _store = store;
        _locator = locator;
    }

    public async Task ProjectAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var id = _locator.GetReadModelId(@event);
        var readModel = await _store.GetByIdAsync(id, cancellationToken);
        var isNew = readModel is null;

        readModel ??= new TReadModel();

        if (readModel is not IAmReadModelFor<TEvent> projection)
        {
            throw new InvalidOperationException(
                $"Read model '{typeof(TReadModel).Name}' does not implement " +
                $"{nameof(IAmReadModelFor<TEvent>)}<{typeof(TEvent).Name}>.");
        }

        var context = new ReadModelContext(isNew);
        projection.Apply(@event, context);

        if (context.IsMarkedForDeletion)
        {
            if (!isNew)
            {
                await _store.Delete(id, cancellationToken);
            }
        }
        else if (isNew)
        {
            await _store.Insert(readModel, cancellationToken);
        }
        else
        {
            await _store.Update(readModel, cancellationToken);
        }
    }
}
