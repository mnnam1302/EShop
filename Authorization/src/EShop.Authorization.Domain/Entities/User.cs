using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class User : AggregateRoot<string>, IExcludedFromScoping
{

    [MaxLength(ModelConstants.ShortText)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string HashedPassword { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string Status { get; set; } = nameof(UserStatus.Inactive);

    [MaxLength(ModelConstants.ShortText)]
    public string CreatedByUserId { get; set; } = string.Empty;

    // Organization relationship
    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    // Roles relationship: https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#many-to-many-with-navigations-to-join-entity
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    private readonly List<UserRole> _userRoles = [];
    public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;

    public static User CreateOwnerUser(
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
            Id = ownerUsername,
            Username = ownerUsername,
            HashedPassword = hashedPassword,
            Name = ownerDisplayName,
            Email = ownerEmail,
            Status = nameof(UserStatus.Inactive),
            OrganizationId = organizationId,
            CreatedByUserId = createdByUserId,
            TenantId = organizationId,
            Scope = organizationId
        };

        user.RaiseDomainEvent(new DomainEvents.UserCreatedDomainEvent
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            RawPassword = randomPassword
        });

        return user;
    }

    public static User Invite(
        string username,
        string randomPassword,
        string hashedPassword,
        string email,
        string displayName,
        string phoneNumber,
        string organizationId,
        string tenantId,
        string createdByUserId)
    {
        var user = new User
        {
            Id = username,
            Username = username,
            HashedPassword = hashedPassword,
            Name = displayName,
            Email = email,
            PhoneNumber = phoneNumber,
            Status = nameof(UserStatus.Inactive),
            OrganizationId = organizationId,
            CreatedByUserId = createdByUserId,
            TenantId = tenantId,
            Scope = tenantId
        };

        user.RaiseDomainEvent(new DomainEvents.UserCreatedDomainEvent
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            RawPassword = randomPassword
        });

        return user;
    }

    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
        {
            return;
        }

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
}