using Eshop.Shared.DomainTools.Aggregates;
using EShop.Identity.Domain.Abstractions.Entities;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Organization : AggregateRoot<string>, IScoped
{
    public Organization()
    {
    }

    public Organization(string name, string? organizationNumber, string? phoneNumber, string? email, string? address, string? city, string? postcode, string? description,  string? parentOrganizationId)
    {
        Id = name;
        Name = name;
        OrganizationNumber = organizationNumber;
        PhoneNumber = phoneNumber;
        Email = email;
        Address = address;
        City = city;
        Postcode = postcode;
        Description = description;
        ParentOrganizationId = parentOrganizationId;
        TenantId = name;
        Scope = name;
    }

    public static Organization Create(Command.CreateOrganization command)
    {
        var organization = new Organization(
            command.Name, 
            command.OrganizationNumber, 
            command.PhoneNumber, 
            command.Email, 
            command.Address, 
            command.City, 
            command.PostCode, 
            command.Description, 
            command.ParentOrganizationId);

         //Raise Domain Event
         organization.RaiseDomainEvent(new DomainEvent.OrganizationCreated
         {
             EventId = Guid.NewGuid(),
             TimeStamp = DateTimeOffset.Now,
             SourceId = organization.Id,
             Name = organization.Name,
             OrganizationNumber = organization.OrganizationNumber,
             PhoneNumber = organization.PhoneNumber,
             Email = organization.Email,
             Address = organization.Address,
             City = organization.City
         });

        return organization;
    }

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; }

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

    [MaxLength(ModelConstants.ShortText)]
    public string? ParentOrganizationId { get; set; }
    public virtual Organization? ParentOrganization { get; set; }

    public virtual List<User>? Users { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? TenantId { get; set; }

    [MaxLength(ModelConstants.LongText)]
    public string? Scope { get; set; }
}