using Eshop.Shared.DomainTools.Aggregates;
using Eshop.Shared.DomainTools.Extensions;
using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Identity.Domain.Entities;

public class Organization : AggregateRoot<string>, IExcludedFromScoping
{
    public Organization()
    {
        // Empty constructor for ORMs
    }

    public Organization(string name, 
        string? organizationNumber, 
        string? phoneNumber, 
        string? email,
        string? address, 
        string? city, 
        string? postcode, 
        string? description, 
        string? parentOrganizationId = null)
    {
        Id = name;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        OrganizationNumber = organizationNumber;
        PhoneNumber = phoneNumber;
        Email = email;
        Address = address;
        City = city;
        Postcode = postcode;
        Description = description;
        ParentOrganizationId = parentOrganizationId;
        Users = new List<User>();
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

    public void AddUser(User user)
    {
        ValidateUser(user);
        EnsureUserDoesNotExist(user);

        Users.Add(user);
    }

    private void ValidateUser(User user)
    {
        if (user == null)
        {
            throw new BadRequestException("User must be required");
        }
    }

    private void EnsureUserDoesNotExist(User user)
    {
        if (Users.Any(u => u.Id == user.Id))
        {
            throw new ConflictException("User already exists");
        }
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

    public virtual List<User>? Users { get; set; } = new List<User>();
}