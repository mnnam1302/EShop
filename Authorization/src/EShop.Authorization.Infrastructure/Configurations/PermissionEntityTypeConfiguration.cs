using EShop.Authorization.Domain;
using EShop.Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Authorization.Infrastructure.Configurations;

internal sealed class PermissionEntityTypeConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Name).IsUnique();

        builder.Property(p => p.Id)
            .HasMaxLength(ModelConstants.ShortText)
            .IsRequired();
    }
}
