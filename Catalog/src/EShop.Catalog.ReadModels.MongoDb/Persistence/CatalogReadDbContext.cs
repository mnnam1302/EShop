using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EShop.Catalog.ReadModels.MongoDb.Persistence;

public sealed class CatalogReadDbContext : DbContext, IInboxDbContext
{
    private readonly string? _tenantId;

    public CatalogReadDbContext(DbContextOptions<CatalogReadDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantId = tenantProvider.TenantId;
    }

    public DbSet<Category> Categories { get; set; } = null!;

    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogReadDbContext).Assembly);

        // Global query filter for tenant isolation on all IScoped entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IScoped).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(BuildTenantFilter(entityType.ClrType));
            }
        }
    }

    /// <summary>
    /// Builds a dynamic lambda expression for tenant filtering: <c>e => e.TenantId == _tenantId</c>.
    /// This is equivalent to writing <c>.HasQueryFilter(e => e.TenantId == tenantId)</c> on each
    /// <see cref="IScoped"/> entity, but constructed at runtime so it can be applied generically
    /// across all scoped entity types discovered in the model.
    /// </summary>
    private LambdaExpression BuildTenantFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var tenantIdProperty = Expression.Property(parameter, nameof(IScoped.TenantId));
        var tenantIdValue = Expression.Constant(_tenantId);
        var comparison = Expression.Equal(tenantIdProperty, tenantIdValue);
        return Expression.Lambda(comparison, parameter);
    }
}
