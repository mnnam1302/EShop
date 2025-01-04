using System.ComponentModel.DataAnnotations;

namespace Eshop.Shared.DomainTools.Entities;

public interface IEntityBase<TKey>
{
    TKey Id { get; }
}

public abstract class EntityBase<TKey> : IEntityBase<TKey>
{
    [MaxLength(ModelConstants.ShortText)]
    public TKey Id { get; set; }
}