using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Abstractions.Entities;

public class EntityBase<TKey> : IEntityBase<TKey>
{
    [MaxLength(ModelConstants.ShortText)]
    public TKey Id { get; set; }
}