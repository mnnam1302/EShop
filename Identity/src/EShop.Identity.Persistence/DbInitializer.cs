using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EShop.Identity.Persistence;

public class DbInitializer
{
    private readonly UsersDbContext _dbContext;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantIsolationStrategy _tenantIsolationStrategy;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    private const string TenantName = "eshop-staging";
    private const string RoleName = "Owner";
    private const string UserName = "owner.staging@gmail.com";
    private const string DisplayName = "Owner Staging";

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
            _userDetailsProvider.SetSystemUserContext(TenantName);

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

            await SeedDataForEShopSystem();
            await SeedDataForSpecificTenant();
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

    /// <summary>
    /// Seed data for system with eshop-support that include wide permissions, system user, maybe support user
    /// </summary>
    /// <returns></returns>
    private async Task SeedDataForEShopSystem()
    {
        await SeedSystemWidePermissionsAsync();
        await SeedTenantAsync(UserData.EShopSupportGroup);
        await SeedOrganizationAsync(UserData.EShopSupportGroup, "Root System Organization", null);
        await SeedUserAsync(
            UserData.SystemUsername,
            $"{UserData.SystemUsername}.{UserData.EShopSupportGroup}@gmail.com",
            "System User",
            UserData.EShopSupportGroup);
    }

    /// <summary>
    /// Seed data for specific tenant
    /// </summary>
    /// <returns></returns>
    private async Task SeedDataForSpecificTenant()
    {
        await SeedTenantAsync(TenantName);
        await SeedOrganizationAsync(TenantName, "Root Organization", UserData.EShopSupportGroup);
        await SeedUserAsync(UserName, UserName, DisplayName, TenantName); // user
        await SeedRoleAsync(RoleName, TenantName); // role, role permissions, user roles
    }

    private async Task SeedTenantAsync(string tenantName)
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

    private async Task SeedOrganizationAsync(string tenantName, string organizationDescription, string? parentOrganization)
    {
        var organization = CreateOrganization(tenantName, organizationDescription, parentOrganization);
        if (await _dbContext.Organizations.AnyAsync(org => org.Id == tenantName || org.Name == tenantName))
        {
            _dbContext.Update(organization);
        }
        else
        {
            _dbContext.Add(organization);
        }
    }

    private async Task<Organization> CreateOrganization(string tenantName, string description, string? parentOrganization = null)
    {
        var organization = new Organization(tenantName,
            new Random().Next(0, 1000000000).ToString(),
            "+477" + new Random().Next(0, 1000000000).ToString(),
            $"{tenantName}@gmail.com",
            string.Empty,
            string.Empty,
            string.Empty,
            description,
            parentOrganization);

        if (!string.IsNullOrWhiteSpace(parentOrganization) && await _dbContext.Organizations.AnyAsync(org => org.Name == parentOrganization))
        {
            organization.ParentOrganizationId = parentOrganization;
        }

        return organization;
    }

    private async Task SeedUserAsync(string userName, string email, string displayName, string tenantName)
    {
        var user = new User(
            userName,
            _passwordHasher.Hash("P@ssword123"),
            email,
            displayName,
            "+477" + new Random().Next(0, 1000000000),
            DateTime.UtcNow.AddYears(-20),
            tenantName)
        {
            CreatedOnUtc = DateTime.UtcNow
        };

        if (await _dbContext.Users.AnyAsync(u => u.Id == userName))
        {
            _dbContext.Update(user);
        }
        else
        {
            _dbContext.Add(user);
        }
    }

    private async Task SeedRoleAsync(string roleName, string tenantName)
    {
        if (!await _dbContext.Roles.AnyAsync(r => r.Name == roleName && r.TenantId == tenantName))
        {
            var role = new Role(Guid.NewGuid(), roleName, "Role owner for tenant initialization");

            await SeedRolePermissions(role, GetWidePermissions());
            await SeedUserRolesAsync(role, UserName);
            _dbContext.Add(role);
        }
    }

    private async Task SeedRolePermissions(Role role, Permission[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (!await _dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id))
            {
                role.RolePermissions.Add((new RolePermission { RoleId = role.Id, PermissionId = permission.Id }));
            }
        }
    }

    private async Task SeedUserRolesAsync(Role role, string userName)
    {
        if (!await _dbContext.UserRoles.AnyAsync(ur => ur.UserId == userName && ur.RoleId == role.Id))
        {
            role.UserRoles.Add(new UserRole { UserId = userName, RoleId = role.Id });
        }
    }

    private async Task SeedSystemWidePermissionsAsync()
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
}