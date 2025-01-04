namespace Eshop.Shared.DomainTools.Entities;

public interface IEntityBase<TKey>
{
    TKey Id { get; }
}