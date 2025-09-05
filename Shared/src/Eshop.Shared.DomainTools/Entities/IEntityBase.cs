namespace EShop.Shared.DomainTools.Entities;

public interface IEntityBase<TKey>
{
    TKey Id { get; }
}

public abstract class EntityBase<TKey> : IEntityBase<TKey>
{
    public required TKey Id { get; set; }
}