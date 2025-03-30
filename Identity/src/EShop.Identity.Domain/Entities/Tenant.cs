using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Tenant : EntityBase<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.MediumText)]
    public string? Name { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? ExternalUsersUserPoolId { get; set; }

    public virtual List<TenantSetting>? TenantSettings { get; set; } = new();
}