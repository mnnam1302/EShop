using EShop.Authorization.Domain.DomainEvents;
using EShop.Authorization.Domain.ValueObjects;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class Organization : AggregateRoot<string>, IExcludedFromScoping
{
    public const int MaxSupportedLevel = 5;

    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? ParentOrganizationId { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? OrganizationNumber { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? Email { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? PhoneNumber { get; set; }

    public OrganisationContext Context { get; set; } = OrganisationContext.Empty();

    public Address? Address { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;

    public virtual Organization? ParentOrganization { get; set; }

    public static Organization CreateRootOrganization(string tenantId, string tenantName)
    {
        var organization = new Organization
        {
            Id = tenantId,
            Name = tenantName,
            Description = "Root Organization",
            ParentOrganizationId = null,
            Context = OrganisationContext.NewRoot(tenantId),
            TenantId = tenantId,
            Scope = tenantId
        };

        // Raise domain event
        organization.RaiseDomainEvent(new OrganizationEvents.RootOrganizationCreated
        {
            EventId = Guid.NewGuid(),
            TimeStamp = DateTimeOffset.UtcNow,
            OrganizationId = organization.Id,
            Name = organization.Name,
            TenantId = tenantId
        });

        return organization;
    }

    public Organization CreateChildOrganization(
        string id,
        string name,
        string email,
        string? description = null,
        string? organizationNumber = null,
        string? phoneNumber = null,
        string? street = null,
        string? city = null,
        string? state = null,
        string? country = null,
        string? zipCode = null)
    {
        var context = OrganisationContext.NewChild(Context);

        var childOrganization = new Organization
        {
            Id = id,
            Name = name,
            Description = description,
            OrganizationNumber = organizationNumber,
            Email = email,
            PhoneNumber = phoneNumber,
            Address = new Address
            {
                Street = street ?? string.Empty,
                City = city ?? string.Empty,
                State = state ?? string.Empty,
                Country = country ?? string.Empty,
                ZipCode = zipCode ?? string.Empty
            },
            ParentOrganizationId = this.Id,
            Context = context,
            TenantId = this.TenantId,
            Scope = context.Path
        };

        // Raise domain event

        return childOrganization;
    }
}
