using EShop.Shared.Scoping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Shared.DbResourceAccessControl.Interceptors;

public sealed class MultiTenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IUserDetailsProvider _userDetailsProvider;

    public MultiTenantSaveChangesInterceptor(IUserDetailsProvider userDetailsProvider)
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

        IEnumerable<EntityEntry<IScoped>> entities =
            dbContext.ChangeTracker
            .Entries<IScoped>();

        foreach (EntityEntry<IScoped> entityEntry in entities)
        {
            entityEntry.Property(x => x.TenantId).CurrentValue = _userDetailsProvider.AuthenticatedUser.TenantId;
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}