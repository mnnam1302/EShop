using EShop.Identity.Domain.Abstractions.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class TenantSetting : EntityBase<string>, IScoped
{
    [MaxLength(ModelConstants.ShortText)]
    public string? TenantId { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Scope { get; set; }

    [MaxLength(ModelConstants.StandardText)]
    public bool CustomerPortalEnabled { get; set; }

    [MaxLength(ModelConstants.StandardText)]
    public string? CustomerPortalUrl { get; set; }
}