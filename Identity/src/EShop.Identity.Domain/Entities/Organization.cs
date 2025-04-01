using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Organization : AggregateRoot<string>, IExcludedFromScoping
{
    public const string DefaultLanguageCode = "en-gb";
    public const string DefaultOwnerPassword = "P@ssword123";
    
    private readonly List<User> _users = new();

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

    public virtual IReadOnlyCollection<User>? Users => _users.AsReadOnly();

    // Empty constructor for ORMs
    public Organization() { }

    public Organization(string id, string name, string? organizationNumber, string? phoneNumber, string? email, string? address, string? city, string? postcode, string? description, string? parentOrganizationId = null)
    {
        Id = id;
        Name = name;
        OrganizationNumber = organizationNumber;
        PhoneNumber = phoneNumber;
        Email = email;
        Address = address;
        City = city;
        Postcode = postcode;
        Description = description;
        ParentOrganizationId = parentOrganizationId;
    }

    public static Organization Create(Command.CreateOrganizationCommand command)
    {
        //var organization = new Organization(
        //    command.Name,
        //    command.Name,
        //    command.OrganizationNumber,
        //    command.PhoneNumber,
        //    command.Email,
        //    command.Address,
        //    command.City,
        //    command.PostCode,
        //    command.Description,
        //    command.ParentOrganizationId);

        return new Organization();
    }

    public static Organization CreateInternal(string tenantId, string name, string? description = null)
    {
        var organization = new Organization
        {
            Id = tenantId,
            Name = name,
            Description = description ?? "Root organization",
            //Context = OrganisationContext.NewRoot(tenantId),
            LanguageCode = DefaultLanguageCode,
            //TenantId = tenantId,
            //Scope = tenantId,
        };

        return organization;
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

    public User AddUser(string username, string password, string displayName, string email, string createdBy)
    {
        // Check if the user is a system user or support group
        if (UserData.IsSystemUser(username) || username.Equals(UserData.EShopSupportGroup, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Invalid username");
        }

        var user = User.CreateInternal(username, password, email, displayName, Id, createdBy);
        _users.Add(user);

        return user;
    }
}