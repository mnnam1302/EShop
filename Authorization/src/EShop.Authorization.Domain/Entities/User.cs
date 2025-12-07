using EShop.Authorization.Domain.StateMachines;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Entities;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class User : AggregateRoot<string>, IExcludedFromScoping
{
    public const int MaxFailedAccessAttemptsBeforeLockout = 5;
    public static readonly TimeSpan DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(15);

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

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;

    public int AccessFailedCount { get; private set; }
    public DateTimeOffset? LockoutEndDateUtc { get; private set; }
    public bool LockoutEnabled { get; private set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public UserStateMachine StateMachine => new(() => ParseStatusSafely(), AfterStateUpdated);

    private UserState ParseStatusSafely()
    {
        if (Enum.TryParse<UserState>(Status, out var s)) return s;
        return UserState.PendingVerification;
    }

    private void AfterStateUpdated(UserState newState)
    {
        Status = Enum.GetName(newState) ?? nameof(UserState.PendingVerification);
    }

    public static User CreateOwnerUser(
        string ownerUsername,
        string randomPassword, // for event only, not stored
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
        if (UserRoles.Any(x => x.RoleId == roleId)) return;

        UserRoles.Add(new UserRole
        {
            UserId = Id,
            RoleId = roleId
        });
    }

    public bool IsLockedOut()
    {
        if (!LockoutEnabled) return false;
        if (!LockoutEndDateUtc.HasValue) return false;

        return LockoutEndDateUtc.Value > DateTimeOffset.UtcNow;
    }

    public int IncrementAccessFailedCount()
    {
        AccessFailedCount++;
        return AccessFailedCount;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
    }

    public void SetLockout(DateTimeOffset lockoutEnd)
    {
        LockoutEnabled = true;
        LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? null : lockoutEnd;
    }

    public void ConfirmInvitation(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentException("Hashed password required", nameof(hashedPassword));
        }

        StateMachine.Fire(UserAction.ConfirmInvitation);
        PasswordHash = hashedPassword;
    }
}