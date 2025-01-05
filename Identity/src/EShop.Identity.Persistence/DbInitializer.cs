using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace EShop.Identity.Persistence;

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

            await SeedSystemWidePermissions();
            await SeedSupportUserForSystem();
            await SeedInitialDataForTenant(); // consider, other solution better
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

    private const string TenantName = "eshop-staging";
    private const string RoleName = "Owner";
    private const string UserName = "owner.staging@gmail.com"; // need other solution, need to username unique for each tenant
    private const string DisplayName = "Owner Staging";

    /// <summary>
    /// Support user support to create and manage tenant, organizaiton, tenant feature, permissions
    /// </summary>
    /// <returns></returns>
    private async Task SeedSupportUserForSystem()
    {
        await SeedOrganization(UserData.EShopSupportGroup, "Support for all organization");
        await SeedOrganizationUser($"{UserData.EShopSupportGroup}@gmail.com", "Support User", UserData.EShopSupportGroup);

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seed an organizaion, user that manage their organization, role for specific tenant
    /// </summary>
    /// <returns></returns>
    private async Task SeedInitialDataForTenant()
    {
        await SeedTenant();
        await SeedOrganization(TenantName, "Root organization");
        await _dbContext.SaveChangesAsync();

        await SeedRole(RoleName, TenantName);
        await SeedOrganizationUser(UserName, DisplayName, TenantName);
        await _dbContext.SaveChangesAsync();

        await SeedUserRoles(UserName, RoleName);
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
        };
    }

    private async Task SeedTenant(string tenantName = TenantName)
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

    private async Task SeedOrganization(string tenantName, string desciption)
    {
        var organization = new Organization()
        {
            Id = tenantName,
            Name = tenantName,
            Description = desciption,
            Email = $"{tenantName}@gmail.com",
            PhoneNumber = "+477" + new Random().Next(0, 1000000000),
        };

        if (!await _dbContext.Organizations.AnyAsync(org => org.Name == organization.Name))
        {
            _dbContext.Add(organization);
        }
        else
        {
            _dbContext.Update(organization);
        }
    }

    private async Task SeedRole(string roleName, string tenantName)
    {
        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(x => x.Name == roleName);

        if (role != null && await _dbContext.Organizations.AnyAsync())
        {
            await SeedRolePermissions(role, GetWidePermissions());
        }
        else
        {
            var newRole = new Role(Guid.NewGuid(), roleName, "Owner of the account");
            newRole.TenantId = tenantName;
            newRole.Scope = tenantName;

            await SeedRolePermissions(newRole, GetWidePermissions());
            _dbContext.Add(newRole);
        }
    }

    private async Task SeedRolePermissions(Role role, Permission[] permissions)
    {
        foreach (var permission in permissions)
        {
            if (!await _dbContext.RolePermissions.AnyAsync(x => x.RoleId == role.Id && x.PermissionId == permission.Id))
            {
                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }
        }
    }

    private async Task SeedOrganizationUser(string userName, string displayName, string tenantName)
    {
        var organization = await _dbContext.Organizations.AsNoTracking()
            .Where(x => x.Name == tenantName)
            .FirstOrDefaultAsync();

        if (organization != null
            && !await _dbContext.Users.AnyAsync(u => u.Id == userName || u.Username == userName))
        {
            var user = new User(
                userName,
                _passwordHasher.Hash("P@ssword"),
                userName,
                displayName,
                "+477" + new Random().Next(0, 1000000000),
                DateTime.UtcNow.AddYears(-20));

            user.CreatedBy = _userDetailsProvider.AuthenticatedUser.ActionUserId;
            user.CreatedOnUtc = DateTime.UtcNow;
            user.AssignOrganization(tenantName);

            _dbContext.Add(user);
        }
    }

    private async Task SeedUserRoles(string userName, string roleName)
    {
        var user = await _dbContext.Users
            .Where(u => u.Username == userName)
            .FirstOrDefaultAsync();

        var role = await _dbContext.Roles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == roleName);

        if (user != null && role != null
            && !await _dbContext.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id))
        {
            user.AssignRole(role.Id);
        }

        await _dbContext.SaveChangesAsync();
    }
}