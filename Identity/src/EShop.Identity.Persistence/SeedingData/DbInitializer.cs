using EShop.Identity.Application.Abstractions;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;

namespace EShop.Identity.Persistence.SeedingData;

public class DbInitializer
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    private const string testOrganizationName = "eshop-stagging";
    private const string testUserName = "kodi.mai@seamless.insure";
    private const string testRoleName = "Admin";

    public DbInitializer(
        UserDbContext dbContext,
        ILogger<DbInitializer> logger,
        IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true, bool applyRingFencing = true)
    {
        try
        {
            if (applyMigrations)
            {
                _logger.LogDebug("Applying any pending migrations...");
                _dbContext.Database.Migrate();
            }
            else
            {
                _logger.LogInformation("Ensuring database is created without running migrations...");
                _dbContext.Database.EnsureCreated();
            }

            // Data system
            await EnsureOrganizationCreated();
            await SeedSystemWidePermissions();
            await SeedSystemAdminRole();
            await SeedAdminUser();
            await _dbContext.SaveChangesAsync();
            
            // Relationships
            await SeedRolePermissionAdmin();
            await AssignRoleUserAdmin();
            await AssignOrganizationForAdminUser();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization error");
        }
    }
    
    private async Task EnsureOrganizationCreated()
    {
        var organization = new Organization()
        {
            Id = testOrganizationName,
            Name = testOrganizationName,
            Description = "Root organization",
            Email = "eshop-stagging@seamless.insure",
            PhoneNumber = "+477" + new Random().Next(0, 1000000000),
        };

        if (await _dbContext.Organizations.AnyAsync(org => org.Name == organization.Name))
        {
            _dbContext.Update(organization);
        }
        else
        {
            _dbContext.Add(organization);
        }
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
                }
        };
    }

    private async Task SeedSystemAdminRole()
    {
        var role = new Role(Guid.NewGuid(), testRoleName, "Admin of the system");

        if (!await _dbContext.Roles.AnyAsync(r => r.Name == role.Name))
        {
            _dbContext.Add(role);
        }
    }

    private async Task SeedRolePermissionAdmin()
    {
        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == testRoleName);

        if (role == null)
        {
            _logger.LogWarning("Role Admin not found");
            return;
        }

        var permissions = GetWidePermissions();

        foreach (var permission in permissions)
        {
            if (role.RolePermissions.Any(rp => rp.PermissionId == permission.Id))
            {
                continue;
            }

            role.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        _dbContext.Update(role);
        await _dbContext.SaveChangesAsync();
    }


    private async Task SeedAdminUser()
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == testUserName || u.Email == testUserName);

        if (user == null)
        {
            var passwordHasher = _serviceProvider.GetRequiredService<IPasswordHasher>();
            user = new User
            {
                Id = testUserName,
                Username = testUserName,
                Email = testUserName,
                DisplayName = "Kodi Mai",
                PasswordHash = passwordHasher.Hash("P@ssword"),
                PhoneNumber = "+477" + new Random().Next(0, 1000000000),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System",
            };

            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task AssignRoleUserAdmin()
    {
        var role = await _dbContext.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Name == testRoleName);
        if (role == null)
        {
            _logger.LogWarning("Role Admin not found");
            return;
        }

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Username == testUserName || u.Email == testUserName);
        if (user == null)
        {
            _logger.LogWarning("User Admin not found");
            return;
        }

        if (!user.UserRoles.Any(x => x.RoleId == role.Id))
        {
            user.UserRoles.Add(new UserRole
            {
                RoleId = role.Id,
                UserId = user.Id
            });
        }

        _dbContext.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    private async Task AssignOrganizationForAdminUser()
    {
        var organization = await _dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Name == testOrganizationName);
        if (organization == null)
        {
            _logger.LogWarning("Organization not found");
            return;
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == testUserName || u.Email == testUserName);
        if (user == null)
        {
            _logger.LogWarning("User admin not found");
            return;
        }

        if (string.IsNullOrEmpty(user.OrganizationId) || user.OrganizationId != organization.Id)
        {
            user.OrganizationId = organization.Id;

            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}