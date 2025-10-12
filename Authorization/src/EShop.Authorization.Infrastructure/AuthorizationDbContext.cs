using EShop.Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Authorization.Infrastructure;

public class AuthorizationDbContext : DbContext
{
    public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Permission> Permissions { get; set; }
}
