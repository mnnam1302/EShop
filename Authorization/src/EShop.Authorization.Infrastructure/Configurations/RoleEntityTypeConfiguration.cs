using EShop.Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Authorization.Infrastructure.Configurations;

internal sealed class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.Name);
        builder.HasIndex(r => r.TenantId);

        builder.HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity<RolePermission>();
    }
}