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

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();

        builder
            .HasOne(x => x.ParentOrganization)
            .WithMany()
            .HasForeignKey(x => x.ParentOrganizationId);

        builder
            .HasMany(x => x.Users)
            .WithOne(x => x.Organization)
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}