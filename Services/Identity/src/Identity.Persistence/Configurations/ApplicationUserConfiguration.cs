using Identity.Domain.Entities;
using Identity.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Configurations;

internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable(TableNames.ApplicationUsers);

        builder.HasKey(x => x.Id);

        //builder
        //    .HasIndex(x => new { x.TenantId, x.Username }).IsUnique();

        builder
            .HasOne(x => x.Organization)
            .WithMany()
            .HasForeignKey(x => x.OrganizationId);
    }
}