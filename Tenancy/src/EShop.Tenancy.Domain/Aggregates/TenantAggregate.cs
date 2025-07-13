using EShop.Shared.DomainTools.Aggregates;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Aggregates;

public class TenantAggregate : AggregateRoot<string>
{
    [MaxLength(ModelConstants.ShortText)]
    public override string Id { get; set; } = string.Empty;
}