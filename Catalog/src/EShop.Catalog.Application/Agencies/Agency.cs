using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Catalog.Application.Agencies;

public class Agency : IEntityBase<Guid>, IScoped
{
    public Agency(string name, string tenantId)
    {
        Id = Guid.NewGuid();
        Name = name;
        TenantId = tenantId;
        Scope = tenantId;
    }

    [MaxLength(ModelConstants.ShortText)]
    public Guid Id { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; }
}
