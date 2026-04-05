namespace EShop.Shared.ReadModel;

/// <summary>
/// Infrastructure-agnostic persistence for read models.
/// Implement for each storage technology (EF Core, Elasticsearch, etc.).
/// </summary>
/// <typeparam name="TReadModel">The read model type.</typeparam>
public interface IReadModelStore<TReadModel> where TReadModel : class, IReadModel
{
    Task<TReadModel?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task Insert(TReadModel readModel, CancellationToken cancellationToken = default);

    Task Update(TReadModel readModel, CancellationToken cancellationToken = default);

    Task Delete(string id, CancellationToken cancellationToken = default);
}
