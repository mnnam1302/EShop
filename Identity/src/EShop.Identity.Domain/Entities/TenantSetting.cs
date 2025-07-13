using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class TenantSetting : EntityBase<Guid>, IScoped
{
    [MaxLength(ModelConstants.LongText)]
    public bool CustomerPortalEnabled { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? CustomerPortalUrl { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; } = string.Empty;
}