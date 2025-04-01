using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Identity.Persistence;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.EntityFrameworkCore;

namespace EShop.Identity.API;

public class DbInitializer
{
    private readonly UsersDbContext _dbContext;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantIsolationStrategy _tenantIsolationStrategy;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public DbInitializer(
        UsersDbContext userDbContext,
        IUserDetailsProvider userDetailsProvider,
        ITenantIsolationStrategy tenantIsolationStrategy,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<DbInitializer> logger)
    {
        _dbContext = userDbContext;
        _userDetailsProvider = userDetailsProvider;
        _tenantIsolationStrategy = tenantIsolationStrategy;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContextWithEmptyScope();

            if (applyMigrations)
            {
                _logger.LogDebug("Applying any pending migrations...");
                await _dbContext.Database.MigrateAsync();
            }
            else
            {
                _logger.LogInformation("Ensuring database is created without running migrations...");
                await _dbContext.Database.EnsureCreatedAsync();
            }

            if (applyTenantIsolation && _configuration.GetValue<bool>("AllowTenantIsolation", true))
            {
                _tenantIsolationStrategy.AddTenantIsolation(_dbContext);
            }

            await SeedDataForEShopSystemAsync();
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task SeedDataForEShopSystemAsync()
    {
        await SeedSystemWidePermissions();

        // System user should not depend on any group like eshop-support or any tenant
        await SeedSystemUser(UserData.SystemUsername, $"{UserData.SystemUsername}@eshop.ecommerce", "System user");

        // Group eshop-support is a tenant, so users can support system and tenant
        await SeedTenant(UserData.EShopSupportGroup);
        await SeedOrganization(
            UserData.EShopSupportGroup,
            $"{UserData.EShopSupportGroup.ToLowerInvariant()}@eshop.ecommerce",
            "Root system organization");
        await SeedSupportUser(
            UserData.EShopSupportGroup,
            $"{UserData.EShopSupportGroup.ToLowerInvariant()}@eshop.ecommerce",
            "Support User",
            UserData.EShopSupportGroup);
    }

    private async Task SeedSystemWidePermissions()
    {
        var permissions = GetWidePermissions();

        foreach (var permission in permissions)
        {
            if (await _dbContext.Permissions.AnyAsync(p => p.Id == permission.Id))
            {
                _dbContext.Update(permission);
            }
            else
            {
                _dbContext.Add(permission);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    private Permission[] GetWidePermissions()
    {
        return new Permission[]
        {
            new Permission
            {
                Id = PermissionConstants.ViewSystemSettingsPermissionId,
                Name = "View system settings",
                Description = "Allows users to view system settings",
                RelatedTo = "System Settings",
            },
            new Permission
            {
                Id = PermissionConstants.ManageSystemSettingsPermissionId,
                Name = "Manage system settings",
                Description = "Allows users to view, edit system settings",
                RelatedTo = "System Settings",
            },
            new Permission
            {
                Id = PermissionConstants.ViewOrganizationsPermissionId,
                Name = "View organizations",
                Description = "Allows users to view organizations",
                RelatedTo = "Organization Management",
            },
            new Permission
            {
                Id = PermissionConstants.ManageOrganizationsPermissionId,
                Name = "Manage organizations",
                Description = "Allows users to view, edit, delete organizations",
                RelatedTo = "Organization Management",
            },
            new Permission
            {
                Id = PermissionConstants.ViewRolesPermissionId,
                Name = "View roles",
                Description = "Allows users viewing roles list and their details",
                RelatedTo = "Role Management",
            },
            new Permission
            {
                Id = PermissionConstants.ManageRolesPermissionId,
                Name = "Manage roles",
                Description = "Allows users to add, create and delete roles",
                RelatedTo = "Role Management",
            },
            new Permission
            {
                Id = PermissionConstants.ViewUsersPermissionId,
                Name = "View users",
                Description = "Allows listing of users and organizations currently registered in the system",
                RelatedTo = "User Management",
            },
            new Permission
            {
                Id = PermissionConstants.ManageUsersPermissionId,
                Name = "Manage users",
                Description = "Allows inviting new users, adding new organizations to the system and changing their details",
                RelatedTo = "User Management",
            },
            new Permission
            {
                Id = PermissionConstants.ViewPortalUserAccountsPermissionId,
                Name = "View portal user accounts",
                Description = "Allows viewing portal user accounts.",
                RelatedTo = "User Management"
            },
            new Permission
            {
                Id = PermissionConstants.ManagePortalUserAccountsPermissionId,
                Name = "Manage portal user accounts",
                Description = "Allows viewing, inviting, updating, and deleting portal user accounts.",
                RelatedTo = "User Management"
            },
            new Permission
            {
                Id = PermissionConstants.ViewCustomerUsersPermissionId,
                Name = "View customer users",
                Description = "Allow list of customer users in the system",
                RelatedTo = "User management"
            },
            new Permission
            {
                Id = PermissionConstants.ManageCustomerUsersPermissionId,
                Name = "Manage customer users",
                Description = "Allows update common information, active, inactive customer users in the system",
                RelatedTo = "User management"
            }
        };
    }

    private async Task SeedSystemUser(string username, string email, string displayName)
    {
        var defaultPassword = _passwordHasher.Hash(Organization.DefaultOwnerPassword);
        var user = User.CreateInternal(username, defaultPassword, email, displayName);

        if (await _dbContext.Users.AnyAsync(u => u.Id == username))
        {
            _dbContext.Update(user);
        }
        else
        {
            _dbContext.Add(user);
        }
    }

    private async Task SeedTenant(string tenantName)
    {
        var tenant = new Tenant()
        {
            Id = tenantName,
            Name = tenantName,
        };

        if (await _dbContext.Tenants.AnyAsync(t => t.Id == tenantName || t.Name == tenantName))
        {
            _dbContext.Update(tenant);
        }
        else
        {
            _dbContext.Add(tenant);
        }
    }

    private async Task SeedOrganization(string name, string email, string description)
    {
        var organization = Organization.CreateInternal(name, name, description);
        organization.Email = email;

        if (await _dbContext.Organizations.AnyAsync(org => org.Id == name || org.Name == name))
        {
            _dbContext.Update(organization);
        }
        else
        {
            _dbContext.Add(organization);
        }
    }

    private async Task SeedSupportUser(string username, string email, string displayName, string tenantName)
    {
        var defaultPassword = _passwordHasher.Hash(Organization.DefaultOwnerPassword);
        var user = User.CreateInternal(username, defaultPassword, email, displayName, tenantName, UserData.SystemUsername);

        if (await _dbContext.Users.AnyAsync(u => u.Id == username))
        {
            _dbContext.Update(user);
        }
        else
        {
            _dbContext.Add(user);
        }
    }
}