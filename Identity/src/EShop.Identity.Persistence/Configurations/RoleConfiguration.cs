using EShop.Identity.Domain.Entities;
using EShop.Identity.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShop.Identity.Persistence.Configurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(TableNames.Roles);

        builder.HasKey(x => x.Id);

        //builder
        //    .HasIndex(x => new { x.TenantId, x.Name }).IsUnique();

        builder
            .HasMany(x => x.Permissions)
            .WithMany(x => x.Roles)
            .UsingEntity<RolePermission>();

        builder
            .HasMany(x => x.Users)
            .WithMany(x => x.Roles)
            .UsingEntity<UserRole>();

        // Referecnce: https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many#understanding-many-to-many-relationships
        //builder
        //    .HasMany(x => x.Permissions)
        //    .WithMany(x => x.Roles)
        //    .UsingEntity<RolePermission>(
        //        j => j
        //            .HasOne(x => x.Permission)
        //            .WithMany()
        //            .HasForeignKey(x => x.PermissionId),
        //        j => j
        //            .HasOne(x => x.Role)
        //            .WithMany()
        //            .HasForeignKey(x => x.RoleId),
        //        j => j.HasKey(x => new { x.RoleId, x.PermissionId}));
    }
}