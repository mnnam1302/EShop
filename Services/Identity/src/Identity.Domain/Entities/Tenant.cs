using Identity.Domain.Abstractions.Entities;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Entities;

public class Tenant : EntityBase<string>
{
    [MaxLength(ModelConstants.MediumText)]
    public string? Name { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? ExternalUsersUserPoolId { get; set; }

    public virtual List<TenantSetting>? TenantSettings { get; set; } = new();
}