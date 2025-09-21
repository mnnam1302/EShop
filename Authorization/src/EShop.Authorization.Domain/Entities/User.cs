using EShop.Shared.DomainTools.Aggregates;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class User : AggregateRoot<string>
{
    [MaxLength(ModelConstants.ShortText)]
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

    [MaxLength(ModelConstants.ShortText)]
    public string CreatedByUserId { get; set; } = string.Empty;

    private readonly List<UserRole> _userRoles = new();
    public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static User Create(
        string ownerUsername,
        string randomPassword,
        string hashedPassword,
        string ownerEmail,
        string ownerDisplayName,
        string organizationId,
        string createdByUserId)
    {
        var user = new User
        {
            Username = ownerUsername,
            HashedPassword = hashedPassword,
            Name = ownerDisplayName,
            Status = nameof(UserStatus.Inactive),
            OrganizationId = organizationId,
            CreatedByUserId = createdByUserId
        };

        return user;
    }

    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return;

        var userRole = new UserRole
        {
            UserId = Id,
            RoleId = roleId
        };

        _userRoles.Add(userRole);
    }
}

public enum UserStatus
{
    Inactive,
    Active,
    Suspended
}