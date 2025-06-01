using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Organization : EntityBase<string>, IExcludedFromScoping
{
    public const string DefaultLanguageCode = "en-gb";
    public const string DefaultOwnerPassword = "P@ssword123";
    public const int MaxSupportedLevel = 5;

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationNumber { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Address { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? City { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public string? Postcode { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.TinyText)]
    public string LanguageCode { get; set; } = DefaultLanguageCode;

    public OrganisationContext Context { get; set; } = OrganisationContext.Empty();

    [MaxLength(ModelConstants.ShortText)]
    public string? ParentOrganizationId { get; set; }

    public virtual Organization? ParentOrganization { get; set; }

    public virtual ICollection<User> Users { get; set; } = [];

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.LongText)]
    public string Scope { get; set; } = string.Empty;

    // Empty constructor for ORMs
    public Organization() { }

    public Organization(string id, string name, string? organizationNumber)
    {
        Id = id;
        Name = name;
        OrganizationNumber = organizationNumber;
    }

    public static Organization CreateRootOrganizationInternal(string tenantId, string name, string? description = null)
    {
        var organization = new Organization
        {
            Id = tenantId,
            Name = name,
            Description = description ?? "Root organization",
            Context = OrganisationContext.NewRoot(tenantId),
            LanguageCode = DefaultLanguageCode,
            TenantId = tenantId,
            Scope = tenantId,
        };

        return organization;
    }

    public Organization CreateChildOrganization(Command.CreateOrganizationCommand command)
    {
        if (string.IsNullOrEmpty(command.ParentOrganizationId))
        {
            throw new BadRequestException("Parent organization ID is required.");
        }

        var childOrganization = new Organization(command.Id, command.Name, command.OrganizationNumber)
        {
            PhoneNumber = command.PhoneNumber,
            Email = command.Email,
            Address = command.Address,
            City = command.City,
            Postcode = command.PostCode,
            Description = command.Description,
            ParentOrganizationId = command.ParentOrganizationId
        };

        var context = OrganisationContext.NewChild(Context);
        childOrganization.Context = context;
        childOrganization.Scope = context.Path;
        childOrganization.TenantId = TenantId;

        return childOrganization;
    }

    public void Update(Command.UpdateOrganizationCommand command)
    {
        Name = command.Name;
        OrganizationNumber = command.OrganizationNumber;
        PhoneNumber = command.PhoneNumber;
        Email = command.Email;
        Address = command.Address;
        City = command.City;
        Postcode = command.PostCode;
        Description = command.Description;
        ParentOrganizationId = command.ParentOrganizationId;
    }
}