using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DomainTools.Entities;
using EShop.Shared.EventBus;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EShop.Catalog.ReadModels.MongoDb.Persistence;

public sealed class CatalogReadDbContext : DbContext, IInboxDbContext
{
    private readonly IUserDetailsProvider _userDetailsProvider;

    public CatalogReadDbContext(DbContextOptions<CatalogReadDbContext> options, IUserDetailsProvider userDetailsProvider)
        : base(options)
    {
        _userDetailsProvider = userDetailsProvider;
    }

    public string TenantId => _userDetailsProvider.AuthenticatedUser.TenantId;

    public DbSet<Category> Categories { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<InboxMessage> InboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogReadDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IScoped).IsAssignableFrom(entityType.ClrType))
                continue;

            var filter = BuildTenantFilter(entityType.ClrType);

            modelBuilder
                .Entity(entityType.ClrType)
                .HasQueryFilter(filter);
        }
    }

    private LambdaExpression BuildTenantFilter(Type entityType)
    {
        // e
        var parameter = Expression.Parameter(entityType, "e");

        // e.TenantId
        var tenantProperty = Expression.Property(parameter, nameof(IScoped.TenantId));

        // this.TenantId
        var tenantId = Expression.Property(
            Expression.Constant(this),
            nameof(TenantId));

        // e.TenantId == this.TenantId
        var body = Expression.Equal(tenantProperty, tenantId);

        return Expression.Lambda(body, parameter);
    }
}