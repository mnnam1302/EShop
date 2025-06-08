using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EShop.Identity.Domain.Entities;

public class User : EntityBase<string>, IDateTracking, IExcludedFromScoping
{
    [MaxLength(ModelConstants.MediumText)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MediumText)]
    public string? DisplayName { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(ModelConstants.StandardText)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public bool IsDirector { get; set; } = false;

    public bool IsHeadOfDepartment { get; set; } = false;

    [MaxLength(ModelConstants.ShortText)]
    public string? ManagerId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationId { get; private set; }

    public virtual Organization? Organization { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(ModelConstants.ShortText)]
    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }

    public DateTimeOffset? LastModifiedOnUtc { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = [];

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public User() { }

    public User(string username, string password, string email, string? displayName)
    {
        Id = username;
        Username = username;
        PasswordHash = password;
        Email = email;
        DisplayName = displayName;
        CreatedOnUtc = DateTimeOffset.UtcNow;
    }

    public static User Create(Command.RegisterUser command)
    {
        var user = Create(command.Username, command.Password, command.Email, command.DisplayName, command.OrganizationId);
        
        user.PhoneNumber = command.PhoneNumber;
        user.DateOfBirth = command.DateOfBirth;

        return user;
    }

    public static User Create(string username, string password, string email, string displayName, string? organizationId = null, string? createdBy = null)
    {
        AssertUsername(username);

        var user = new User(username, password, email, displayName);
        user.CreatedBy = createdBy;
        
        if (!string.IsNullOrWhiteSpace(organizationId))
        {
            user.OrganizationId = organizationId;
        }
        
        return user;
    }

    private static void AssertUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or whitespace");
        }

        var invalidCharactersPattern = @"[<>;&/\\\s]";
        if (System.Text.RegularExpressions.Regex.IsMatch(username, invalidCharactersPattern))
        {
            throw new ArgumentException("Username cannot contain special characters");
        }

        if (UserData.IsSystemUser(username) || username.Equals(UserData.EShopSupportGroup, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid username");
        }
    }

    public Claim[] GenerateClaims()
    {
        return
        [
            new Claim("sub", Id),
            new Claim("username", Username),
            new Claim("tenant:groups", OrganizationId ?? string.Empty)
        ];
    }

    public void GrantRoles(string[] roleIds)
    {
        foreach (var roleId in roleIds)
        {
            GrantRole(roleId);
        }
    }

    public void GrantRole(string roleId)
    {
        var userRole = new UserRole()
        {
            RoleId = roleId,
            UserId = Id
        };

        UserRoles.Add(userRole);
    }
}