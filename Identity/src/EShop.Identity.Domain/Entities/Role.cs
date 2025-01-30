using EShop.Shared.Contracts.Services.Identity.Roles;
using EShop.Shared.DomainTools.DomainExceptions;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Role : EShop.Shared.DomainTools.Entities.EntityBase<string>, IScoped
{
    public Role()
    { }

    public Role(Guid id, string name, string? description = "", string? phoneNumber = "")
    {
        Id = $"role-{id}";
        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
    }

    public static Role Create(Command.CreateRoleCommand command)
    {
        var role = new Role(Guid.NewGuid(), command.Name, command.Description, command.PhoneNumber);
        role.AssertRole();

        return role;
    }

    public void Update(string name, string? description, string? phoneNumber)
    {
        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
        AssertRole();
    }

    private void AssertRole()
    {
        ValidateName(Name);
        ValidateDescription(Description);
        ValidatePhoneNumber(PhoneNumber);
    }

    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Role's name must be required");
        }

        if (name.Length > ModelConstants.MediumText)
        {
            throw new BadRequestException($"Role's name must not exceed {ModelConstants.MediumText}");
        }
    }

    private void ValidateDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > ModelConstants.LongText)
        {
            throw new BadRequestException($"Role's description must not exceed {ModelConstants.LongText}");
        }
    }

    private void ValidatePhoneNumber(string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length > ModelConstants.ShortText)
        {
            throw new BadRequestException($"Role's phone number must not exceed {ModelConstants.ShortText}");
        }
    }

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; private set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; private set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    public virtual List<User> Users { get; set; } = new();
    public virtual List<UserRole> UserRoles { get; set; } = new();

    public virtual List<Permission> Permissions { get; set; } = new();
    public virtual List<RolePermission> RolePermissions { get; set; } = new();

    [MaxLength(ModelConstants.ShortText)]
    public string? TenantId { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Scope { get; set; }
}