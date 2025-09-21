using EShop.Shared.DomainTools.Aggregates;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

internal class User : AggregateRoot<string>
{
    [MaxLength(ModelConstants.MediumText)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string HashedPassword { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string Status { get; set; } = nameof(UserStatus.Inactive);

    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationId { get; set; }

    public virtual Organization? Organization { get; set; }
}

public enum UserStatus
{
    Inactive,
    Active,
    Suspended
}