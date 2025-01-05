using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Shared.DbResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using MassTransit;
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

    private const string tenantName = "eshop-staging";
    private const string roleName = "Owner";
    private const string userName = "owner.staging@gmail.com";
    private const string displayName = "Owner Staging";

    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContext(tenantName);

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
            await SeedDataForTenant();
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

    private async Task SeedDataForTenant()
    {
        await SeedTenant();
        //await SeedSystemWidePermissions(); // no belong to tenant, so if put here, it violates `no side effect` in clean code
        await SeedOrganization();
        await SeedRole();
        await SeedUser();
    }

    private async Task SeedTenant()
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

        await _dbContext.SaveChangesAsync();
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

    private async Task SeedOrganization()
    {
        var organization = new Organization()
        {
            Id = tenantName,
            Name = tenantName,
            Description = "Root organization",
            Email = "eshop-stagging@gmail.com",
            PhoneNumber = "+477" + new Random().Next(0, 1000000000),
        };
        organization.TenantId = tenantName;
        organization.Scope = tenantName;

        if (await _dbContext.Organizations.AnyAsync(org => org.Name == organization.Name))
        {
            _dbContext.Update(organization);
        }
        else
        {
            _dbContext.Add(organization);
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedRole()
    {
        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(x => x.Name == roleName);
        if (role == null)
        {
            var newRole = new Role(Guid.NewGuid(), roleName, "Owner of the account");
            newRole.TenantId = tenantName;
            newRole.Scope = tenantName;

            await SeedPermissionsForRole(newRole, GetWidePermissions());
            _dbContext.Add(newRole);
        }
        else
        {
            await SeedPermissionsForRole(role, GetWidePermissions());
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedPermissionsForRole(Role role, Permission[] permissions)
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

    private async Task SeedUser()
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
            user.AssignOrganization(tenantName);

            await SeedRolesForUser(user);
            _dbContext.Add(user);
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedRolesForUser(User user)
    {
        var role = await _dbContext.Roles.AsNoTracking()
            .Where(r => r.Name == roleName)
            .FirstOrDefaultAsync();

        if (role != null 
            && !await _dbContext.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == roleName))
        {
            //user.AssignRole(roleName); // incorrect as RoleId is string that combine "role-Guid"
            user.AssignRole(role.Id);
        }
    }
}