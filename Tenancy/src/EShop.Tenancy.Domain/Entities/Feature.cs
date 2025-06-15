using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Feature : EntityBase<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.ShortMediumText)]
    public string Name { get; set; }

    [MaxLength(ModelConstants.VeryLongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string Module { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string State { get; set; } = nameof(StateFeature.Enabled);

    [MaxLength(ModelConstants.MediumText)]
    public string DefaultStateForNewTenant { get; set; } = nameof(StateFeature.Disabled);

    [MaxLength(ModelConstants.MediumText)]
    public string? Category { get; set; }
}