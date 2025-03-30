using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
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

    public virtual List<Role> Roles { get; set; } = new();
    public virtual List<UserRole> UserRoles { get; set; } = new();

    [MaxLength(ModelConstants.ShortText)]
    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }

    // Empty constructor for ORMs
    public User() { }

    public User(string userName, string password, string email, string? displayName, string organizationId)
    {
        AssertUsername(userName);

        Id = userName;
        Username = userName;
        PasswordHash = password;
        Email = email;
        DisplayName = displayName;
        OrganizationId = organizationId;
    }

    public static User Create(Command.CreateUserCommand command)
    {
        var user = new User(command.Username,
            command.Password,
            command.Email,
            command.DisplayName,
            command.OrganizationId);

        return user;
    }

    public static User Create(string username, string password, string email, string displayName, string organizationId, string createdBy = null)
    {
        var user = new User(username, password, email, displayName, organizationId);
        user.CreatedBy = createdBy;

        return user;
    }

    public Claim[] GenerateClaims()
    {
        return new Claim[]
        {
            new Claim("sub", Id),
            new Claim("username", Username),
            new Claim("tenant:groups", OrganizationId ?? string.Empty)
        };
    }

    public void GrantRoles(IEnumerable<string> roleIds)
    {
        foreach (var roleId in roleIds)
        {
            if (UserRoles.Any(ur => ur.RoleId == roleId))
            {
                throw new BadRequestException($"User already has the role: {roleId}");
            }

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

    private static void AssertUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username cannot be null or whitespace");
        }

        var invalidCharactersPattern = @"[<>;&/\\\s]";
        if (System.Text.RegularExpressions.Regex.IsMatch(username, invalidCharactersPattern))
        {
            throw new UnprocessableEntityException("Username cannot contain special characters");
        }
    }
}