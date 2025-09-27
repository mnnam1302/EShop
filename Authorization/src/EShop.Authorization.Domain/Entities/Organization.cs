using EShop.Authorization.Domain.DomainEvents;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Aggregates;
using EShop.Shared.Scoping;
using System.ComponentModel.DataAnnotations;

namespace EShop.Authorization.Domain.Entities;

public class Organization : AggregateRoot<string>, IExcludedFromScoping
{
    [MaxLength(ModelConstants.MediumText)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? ParentOrganizationId { get; set; }
    public virtual Organization? ParentOrganization { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;

    public static Organization CreateRootOrganization(string tenantId, string tenantName)
    {
        var organization = new Organization
        {
            Id = tenantId,
            Name = tenantName,
            Description = "Root Organization",
            ParentOrganizationId = null,
            TenantId = tenantId,
            Scope = tenantId
        };

        // Raise domain event
        organization.Raise(new OrganizationEvents.RootOrganizationCreated
        {
            EventId = Guid.NewGuid(),
            TimeStamp = DateTimeOffset.UtcNow,
            OrganizationId = organization.Id,
            Name = organization.Name,
            TenantId = tenantId
        });

        return organization;
    }
}
