using EShop.Identity.Domain.Exceptions;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.DomainTools.Extensions;
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

    public static Organization Create(Command.CreateOrganizationCommand command)
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

        RaiseDomainEvent(new DomainEvent.OrganizationUpdated
        {
            EventId = Guid.NewGuid(),
            TimeStamp = DateTimeOffset.Now,
            SourceId = Id,
            Name = Name,
            OrganizationNumber = OrganizationNumber,
            PhoneNumber = PhoneNumber,
            Email = Email,
            Address = Address,
            City = City
        });
    }

    public void AddUser(User user)
    {
        ValidateUser(user);
        EnsureUserDoesNotExist(user);
        Users.Add(user);

        // Raise Domain Event
        RaiseDomainEvent(new Shared.Contracts.Services.Identity.Users.DomainEvent.UserCreated
        {
            EventId = Guid.NewGuid(),
            TimeStamp = DateTimeOffset.Now,
            SourceId = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            OrganizationId = user.OrganizationId!
        });
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