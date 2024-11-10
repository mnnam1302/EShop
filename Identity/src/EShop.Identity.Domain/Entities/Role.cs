using EShop.Identity.Domain.Abstractions.Entities;
using EShop.Identity.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Role : EntityBase<string> //, IScoped
{
    public Role()
    {
    }

    public Role(Guid id, string name, string? description = "", string? phoneNumber = "")
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidatePhoneNumber(phoneNumber);
        Id = $"role-{id}";
        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
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
    [Required]
    public string? Name { get; private set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; private set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    public virtual List<User> Users { get; set; } = new();
    public virtual List<UserRole> UserRoles { get; set; } = new();

    public virtual List<Permission> Permissions { get; set; } = new();
    public virtual List<RolePermission> RolePermissions { get; set; } = new();

    //[MaxLength(ModelConstants.ShortText)]
    //public string? TenantId { get; set; }

    //[MaxLength(ModelConstants.LongText)]
    //public string? Scope { get; set; }
}