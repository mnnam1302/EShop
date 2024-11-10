using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class TenantSetting
{
    [MaxLength(ModelConstants.ShortText)]
    public string? TenantId { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Scope { get; set; }

    public bool CustomerPortalEnabled { get; set; }

    public string? CustomerPortalUrl { get; set; }
}