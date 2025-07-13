using EShop.Configuration.Application.Agencies;
using EShop.Configuration.Application.Products.Create;
using EShop.Configuration.Application.SalesChannels;
using EShop.Shared.Contracts.Shared;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace EShop.Configuration.Application.Products;

public class Product : EntityBase<Guid>, IScoped
{
    public static Product Create(Command command, IUserDetailsProvider userDetailsProvider)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedByUserId = userDetailsProvider.AuthenticatedUser.Id,
            TenantId = userDetailsProvider.AuthenticatedUser.TenantId,
            Scope = userDetailsProvider.AuthenticatedUser.TenantId,
            IsActive = true
        };

        return product;
    }

    public void AssignToAgency(Agency agency)
    {
        ArgumentNullException.ThrowIfNull(agency);

        if (IsArchived)
        {
            throw new BadRequestException("Cannot assign archived products to agencies");
        }

        AgencyId = agency.Id;
    }

    [MaxLength(ModelConstants.MediumText)]
    public required string Name { get; set; }

    [MaxLength(ModelConstants.ShortText)]
    public string? AgencyId { get; private set; }

    public virtual Agency? Agency { get; set; }

    public bool IsActive { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset? ArchivedDate { get; set; }

    public DateTimeOffset? UnarchivedDate { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTimeOffset? LastModifiedAtUtc { get; set; }

    [MaxLength(ModelConstants.MediumText)]
    public string? LastModifiedByUserId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.ShortText)]
    public string TenantId { get; private set; } = string.Empty;

    [MaxLength(ModelConstants.VeryLongText)]
    public string Scope { get; private set; } = string.Empty;

    public virtual ICollection<SalesChannel> SalesChannels { get; set; } = [];

    public virtual ICollection<SalesChannelProduct> SalesChannelProducts { get; set; } = [];

    public virtual ICollection<ProductVersion> Versions { get; set; } = [];
}