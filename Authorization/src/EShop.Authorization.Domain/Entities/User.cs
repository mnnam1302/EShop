using EShop.Authorization.Domain.StateMachines;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class User : AggregateRoot<string>, IExcludedFromScoping
{

    [MaxLength(ModelConstants.ShortText)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string Status { get; set; } = nameof(UserState.PendingVerification);

    [MaxLength(ModelConstants.ShortText)]
    public string CreatedByUserId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = [];

    private readonly List<UserRole> userRoles = [];
    public virtual IReadOnlyCollection<UserRole> UserRoles => userRoles.AsReadOnly();

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;

    /// <summary>
    /// Used to record failures for the purposes of lockout
    /// </summary>
    public virtual int AccessFailedCount { get; set; }

    /// <summary>
    /// DateTime in UTC when lockout ends, any time in the past is considered not locked out.
    /// </summary>
    public virtual DateTimeOffset? LockoutEndDateUtc { get; set; }

    /// <summary>
    /// Is lockout enabled for this user
    /// </summary>
    public virtual bool LockoutEnabled { get; set; }

    public UserStateMachine StateMachine => new(() => Enum.Parse<UserState>(Status), AfterStateUpdated);

    private void AfterStateUpdated(UserState newState)
    {
        Status = Enum.GetName(newState).Require();
    }

    public static User CreateOwnerUser(
        string ownerUsername, string randomPassword, string hashedPassword, string ownerEmail, string ownerDisplayName, string organizationId, string createdByUserId)
    {
        var user = new User
        {
            Id = ownerUsername,
            Username = ownerUsername,
            PasswordHash = hashedPassword,
            Name = ownerDisplayName,
            Email = ownerEmail,
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
        string username, string randomPassword, string hashedPassword, string email, string displayName, string phoneNumber, string organizationId, string tenantId, string createdByUserId)
    {
        var user = new User
        {
            Id = username,
            Username = username,
            PasswordHash = hashedPassword,
            Name = displayName,
            Email = email,
            PhoneNumber = phoneNumber,
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
        if (userRoles.Any(ur => ur.RoleId == roleId))
        {
            return;
        }

        var userRole = new UserRole
        {
            UserId = Id,
            RoleId = roleId
        };

        userRoles.Add(userRole);
    }

    public void ConfirmInvitation(string password)
    {
        StateMachine.Fire(UserAction.ConfirmInvitation);
        PasswordHash = password;
    }
}