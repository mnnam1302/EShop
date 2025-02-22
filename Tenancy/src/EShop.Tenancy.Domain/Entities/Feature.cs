using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Feature
{
    [MaxLength(ModelConstants.ShortMediumText)]
    public string Name { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string Module { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string State { get; set; } = StateFeature.Enabled;

    [MaxLength(ModelConstants.MediumText)]
    public string DefaultStateForNewTenant { get; set; } = StateFeature.Disabled;

    [MaxLength(ModelConstants.MediumText)]
    public string? Category { get; set; }
}