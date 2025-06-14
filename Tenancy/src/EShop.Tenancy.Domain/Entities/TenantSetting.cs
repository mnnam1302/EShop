using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class TenantSetting : EntityBase<Guid>, IScoped
{
    [MaxLength(ModelConstants.ShortText)]
    public string? DisplayDateFormat { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? DisplayTimeFormat { get; set; }

    [MaxLength(ModelConstants.ShortMediumText)]
    public string? TimeZone { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public string? DefaultCurrency { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? CurrencyDisplayFormat { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string DefaultSystemLanguage { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; set; } = string.Empty;
}
