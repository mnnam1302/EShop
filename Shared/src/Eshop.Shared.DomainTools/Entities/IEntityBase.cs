namespace Eshop.Shared.DomainTools.Entities;

public interface IEntityBase<TKey>
{
    TKey Id { get; }
}

public abstract class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}