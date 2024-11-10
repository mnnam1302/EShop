namespace EShop.Identity.Domain.Abstractions.Entities;

public interface IEntityBase<TKey>
{
    TKey Id { get; }
}