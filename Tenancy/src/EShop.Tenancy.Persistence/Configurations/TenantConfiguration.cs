using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Tenancy.Persistence.Configurations;

internal class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable(TableNames.Tenants);

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Description);
        builder.Property(x => x.OwnerUsername);
        builder.Property(x => x.Email);
        builder.Property(x => x.PhoneNumber);

        builder
            .HasMany(t => t.TenantSettings)
            .WithOne()
            .HasForeignKey(ts => ts.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}