using Eshop.Shared.DomainTools.Entities;
using EShop.Shared.Scoping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Shared.DbResourceAccessControl.Interceptors;

public class AuditableInterceptor : SaveChangesInterceptor
{
    private readonly IUserDetailsProvider _userDetailsProvider;

    public AuditableInterceptor(IUserDetailsProvider userDetailsProvider)
    {
        _userDetailsProvider = userDetailsProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        DbContext? dbContext = eventData.Context;

        if (dbContext == null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        UpdateDateTrackingEntities(dbContext);
        UpdateUserTrackingEntities(dbContext);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateDateTrackingEntities(DbContext dbContext)
    {
        IEnumerable<EntityEntry<IDateTracking>> entities = dbContext.ChangeTracker.Entries<IDateTracking>();

        foreach (EntityEntry<IDateTracking> entityEntry in entities)
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(x => x.CreatedOnUtc).CurrentValue = DateTime.UtcNow;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(x => x.LastModifiedOnUtc).CurrentValue = DateTime.UtcNow;
            }
        }
    }

    private void UpdateUserTrackingEntities(DbContext dbContext)
    {
        IEnumerable<EntityEntry<IUserTracking>> userTrackingEntities = dbContext.ChangeTracker.Entries<IUserTracking>();

        foreach (EntityEntry<IUserTracking> entityEntry in userTrackingEntities)
        {
            if (entityEntry.State == EntityState.Added)
            {
                entityEntry.Property(x => x.CreatedBy).CurrentValue = _userDetailsProvider.AuthenticatedUser.ActionUserId;
            }
            else if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(x => x.LastModifiedBy).CurrentValue = _userDetailsProvider.AuthenticatedUser.ActionUserId;
            }
        }
    }
}
