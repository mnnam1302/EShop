using EShop.Authorization.Domain.Entities;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.DbResourceAccessControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Authorization.Infrastructure;

public sealed class DbInitializer(
    AuthorizationDbContext dbContext,
    IUserDetailsProvider userDetailsProvider,
    ITenantIsolationStrategy tenantIsolationStrategy,
    IConfiguration configuration,
    ILogger<DbInitializer> logger)
{
    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            userDetailsProvider.SetSystemUserContextWithEmptyScope();

            if (applyMigrations)
            {
                logger.LogDebug("Applying any pending migrations...");
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                logger.LogInformation("Ensuring database is created without running migrations...");
                await dbContext.Database.EnsureCreatedAsync();
            }

            if (applyTenantIsolation && configuration.GetValue<bool>("AllowTenantIsolation", true))
            {
                tenantIsolationStrategy.AddTenantIsolation(dbContext);
            }

            await SeedSystemWidePermissions();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
        finally
        {
            userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task SeedSystemWidePermissions()
    {
        var permissions = GetWidePermissions();

        foreach (var permission in permissions)
        {
            if (await dbContext.Permissions.AnyAsync(p => p.Id == permission.Id))
            {
                dbContext.Update(permission);
            }
            else
            {
                dbContext.Add(permission);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static Permission[] GetWidePermissions()
    {
        return
        [
            new Permission
            {
                Id = Tenancy.ViewSystemSettings,
                Name = "View system settings",
                Description = "Allows users to view system settings",
                RelatedTo = "System Settings",
            },
            new Permission
            {
                Id = Tenancy.ManageSystemSettings,
                Name = "Manage system settings",
                Description = "Allows users to view, edit system settings",
                RelatedTo = "System Settings",
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ViewOrganizations,
                Name = "View organizations",
                Description = "Allows users to view organizations",
                RelatedTo = "Organization Management",
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ManageOrganizations,
                Name = "Manage organizations",
                Description = "Allows users to view, edit, delete organizations",
                RelatedTo = "Organization Management",
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ViewRoles,
                Name = "View roles",
                Description = "Allows users viewing roles list and their details",
                RelatedTo = "Role Management",
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ManageRoles,
                Name = "Manage roles",
                Description = "Allows users to add, create and delete roles",
                RelatedTo = "Role Management",
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ViewUsers,
                Name = "View users",
                Description = "Allows listing of users and organizations currently registered in the system",
                RelatedTo = "User Management",
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ManageUsers,
                Name = "Manage users",
                Description = "Allows inviting new users, adding new organizations to the system and changing their details",
                RelatedTo = "User Management",
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ViewPortalUserAccounts,
                Name = "View portal user accounts",
                Description = "Allows viewing portal user accounts.",
                RelatedTo = "User Management"
            },
            new Permission
            {
                Id = Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization.ManagePortalUserAccounts,
                Name = "Manage portal user accounts",
                Description = "Allows viewing, inviting, updating, and deleting portal user accounts.",
                RelatedTo = "User Management"
            },
        ];
    }
}
