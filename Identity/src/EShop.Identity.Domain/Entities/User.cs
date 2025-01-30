using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.DomainTools.DomainExceptions;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EShop.Identity.Domain.Entities;

public class User : EntityBase<string>, IDateTracking, IExcludedFromScoping
{
    protected User()
    { }

    public User(string userName, string password, string email, string? displayName, string? phoneNumber, DateTime? dateofBirth, string organizationId)
    {
        Id = userName;
        Username = userName;
        PasswordHash = password;
        Email = email;
        DisplayName = displayName;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateofBirth?.ToUniversalTime();
        OrganizationId = organizationId;
        IsActive = true;
    }

    public static User Create(Command.CreateUserCommand command)
    {
        var user = new User(command.Username,
            command.Password,
            command.Email,
            command.DisplayName,
            command.PhoneNumber,
            command.DateOfBirth,
            command.OrganizationId);

        user.AssertCreateUser();
        return user;
    }

    private void AssertCreateUser()
    {
        this.AssetUserName(Username);
        this.AssetPassword(PasswordHash);
        this.AssetEmail(Email);
        this.AssetDisplayName(DisplayName);
        this.AssetPhoneNumber(PhoneNumber);
        this.AssetDateOfBirth(DateOfBirth);
    }

    private void AssetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new BadRequestException("User name is required");
        }

        if (userName.Length < 6 || userName.Length > 150)
        {
            throw new BadRequestException("User name must be at lease 6 and not exceed 150 characters");
        }
    }

    private void AssetPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new BadRequestException("Password is required");
        }

        if (password.Length < 6 || password.Length > 255)
        {
            throw new BadRequestException("Password must be at lease 6 and not exceed 150 characters");
        }
    }

    private void AssetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Email is required");
        }

        if (!string.IsNullOrEmpty(email) && email.Length > 150)
        {
            throw new BadRequestException("Email is invalid");
        }
    }

    private void AssetDisplayName(string? displayName)
    {
        if (!string.IsNullOrEmpty(displayName) && displayName.Length > 150)
        {
            throw new BadRequestException("Display name is invalid");
        }
    }

    private void AssetPhoneNumber(string? phoneNumber)
    {
        if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length > 50)
        {
            throw new BadRequestException("Phone number is invalid");
        }
    }

    private void AssetDateOfBirth(DateTime? dateOfBirth)
    {
        if (dateOfBirth.HasValue && dateOfBirth.Value > DateTime.Now)
        {
            throw new BadRequestException("Date of birth is invalid");
        }
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

    public void AddRoles(IEnumerable<string> roleIds)
    {
        foreach (var roleId in roleIds)
        {
            if (UserRoles.Any(ur => ur.RoleId == roleId))
            {
                throw new BadRequestException($"User already has the role: {roleId}");
            }

            AssignRole(roleId);
        }
    }

    private void AssignRole(string roleId)
    {
        var userRole = new UserRole()
        {
            RoleId = roleId,
            UserId = Id
        };

        UserRoles.Add(userRole);
    }

    [MaxLength(ModelConstants.MediumText)]
    public string Username { get; private set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? DisplayName { get; private set; }

    [MaxLength(ModelConstants.MediumText)]
    [EmailAddress]
    public string Email { get; private set; }

    [MaxLength(ModelConstants.StandardText)]
    public string PasswordHash { get; private set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; private set; }

    public DateTime? DateOfBirth { get; private set; }

    public bool IsDirector { get; set; } = false;
    public bool IsHeadOfDepartment { get; set; } = false;

    [MaxLength(ModelConstants.ShortText)]
    public string? ManagerId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationId { get; private set; }

    public bool IsActive { get; set; } = true;

    public virtual List<Role> Roles { get; set; } = new();
    public virtual List<UserRole> UserRoles { get; set; } = new();

    [MaxLength(ModelConstants.ShortText)]
    public string? CreatedBy { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
}