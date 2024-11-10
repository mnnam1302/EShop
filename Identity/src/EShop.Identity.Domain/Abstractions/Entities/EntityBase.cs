namespace EShop.Identity.Domain.Abstractions.Entities;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    public TKey Id { get; set; }
}