using EShop.Identity.Domain;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Identity.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable(TableNames.Tenants);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(ModelConstants.ShortText)
            .IsRequired();

        builder
            .HasMany(x => x.TenantSettings)
            .WithOne()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}