using EShop.Shared.DomainTools.Entities;
using EShop.Tenancy.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace EShop.Tenancy.Domain.Entities;

public class Feature : EntityBase<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.ShortMediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string Module { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string State { get; set; } = nameof(StateFeature.Enabled);

    [MaxLength(ModelConstants.MediumText)]
    public string DefaultStateForNewTenant { get; set; } = nameof(StateFeature.Disabled);

    [MaxLength(ModelConstants.MediumText)]
    public string? Category { get; set; }

    public static Feature Create(string id, string name, string? description, string module, string state)
    {
        if (state is not (nameof(StateFeature.Enabled)) and not (nameof(StateFeature.Disabled)))
        {
            throw new ArgumentException("Invalid state value", nameof(state));
        }

        return new Feature
        {
            Id = id,
            Name = name,
            Description = description,
            Module = module,
            State = state,
            DefaultStateForNewTenant = state
        };
    }
}