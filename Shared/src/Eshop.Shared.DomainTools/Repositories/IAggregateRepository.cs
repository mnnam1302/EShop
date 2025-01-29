using EShop.Shared.DomainTools.Aggregates;

namespace EShop.Shared.DomainTools.Repositories;

/// <summary>
/// This interface is used to define a repository for an aggregate root.
/// </summary>
/// <typeparam name="TAggregateRoot"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IAggregateRepository<TAggregateRoot, in TKey> : IRepositoryBase<TAggregateRoot, TKey>
    where TAggregateRoot : class, IAggregateRoot<TKey>
{
}