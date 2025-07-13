using EShop.Identity.Domain;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Identity.Persistence.Configurations;

internal class OrganizaitionConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable(TableNames.Organizations);

        builder.HasKey(o => o.Id);
        builder.HasIndex(o => new { o.TenantId, o.Name }).IsUnique();

        builder.Property(o => o.Id)
            .HasMaxLength(ModelConstants.ShortText)
            .IsRequired();
        builder.OwnsOne(o => o.Context);

        builder
            .HasOne(o => o.ParentOrganization)
            .WithMany()
            .HasForeignKey(x => x.ParentOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(o => o.Users)
            .WithOne(u => u.Organization)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}