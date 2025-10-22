using EShop.Authorization.Domain;
using EShop.Authorization.Domain.Entities;
using EShop.Authorization.Domain.ValueObjects;
using EShop.Shared.Scoping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Authorization.Infrastructure.Configurations;

internal sealed class OrganizationEntityTypeConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.HasIndex(o => new { o.TenantId, o.Name }).IsUnique();

        builder.Property(o => o.Id)
            .HasMaxLength(ModelConstants.ShortText)
            .IsRequired();

        builder.OwnsOne<OrganisationContext>(o => o.Context);

        builder.OwnsOne<Address>(o => o.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasColumnName("Street")
                .HasMaxLength(ModelConstants.MediumText);

            addressBuilder.Property(a => a.City)
                .HasColumnName("City")
                .HasMaxLength(ModelConstants.ShortText);

            addressBuilder.Property(a => a.Country)
                .HasColumnName("Country")
                .HasMaxLength(ModelConstants.ShortText);

            addressBuilder.Property(a => a.State)
                .HasColumnName("State")
                .HasMaxLength(ModelConstants.ShortText);

            addressBuilder.Property(a => a.ZipCode)
                .HasColumnName("ZipCode")
                .HasMaxLength(ModelConstants.ShortText);
        });

        builder
            .HasOne(o => o.ParentOrganization)
            .WithMany()
            .HasForeignKey(x => x.ParentOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
