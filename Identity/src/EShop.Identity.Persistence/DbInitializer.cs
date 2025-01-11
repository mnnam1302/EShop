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

    public async Task Initialize(bool applyMigrations = true, bool applyTenantIsolation = true)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContext(UserData.EShopSupportGroup);

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
    /// Seed data for system, shoukd use within one transaction to consistency data
    /// </summary>
    /// <returns></returns>
    private async Task SeedDataForEShopSystem()
    {
        await SeedTenantAsync(UserData.EShopSupportGroup);
        await SeedOrganizationAsync(UserData.EShopSupportGroup, "Root System Organization");
        await SeedSystemWidePermissionsAsync();

        await _dbContext.SaveChangesAsync();
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

    private async Task SeedOrganizationAsync(string tenantName, string description)
    {
        var organization = CreateOrganization(tenantName, description);
        var user = CreateSystemUser(
            UserData.SystemUsername,
            $"{UserData.SystemUsername}.{tenantName}@gmail.com",
            "System User",
            UserData.EShopSupportGroup);

        var existingOrganization = await _dbContext.Organizations
            .FirstOrDefaultAsync(org => org.Id == tenantName);

        if (existingOrganization == null)
        {
            organization.AddUser(user);
            _dbContext.Add(organization);
        }
        else
        {
            UpdateExistingOrganization(existingOrganization, organization);
        }
    }

    private Organization CreateOrganization(string tenantName, string description)
    {
        return new Organization()
        {
            Id = tenantName,
            Name = tenantName,
            Description = description,
            Email = $"{tenantName}@gmail.com",
            OrganizationNumber = new Random().Next(0, 1000000000).ToString(),
            PhoneNumber = "+477" + new Random().Next(0, 1000000000).ToString()
        };
    }

    private User CreateSystemUser(string userName, string email, string displayName, string tenantName)
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
            CreatedBy = _userDetailsProvider.AuthenticatedUser.ActionUserId,
            CreatedOnUtc = DateTime.UtcNow
        };

        return user;
    }

    private void UpdateExistingOrganization(Organization existingOrganization, Organization newOrganization)
    {
        existingOrganization.Name = newOrganization.Name;
        existingOrganization.Description = newOrganization.Description;
        existingOrganization.Email = newOrganization.Email;
        existingOrganization.PhoneNumber = newOrganization.PhoneNumber;
        existingOrganization.OrganizationNumber = newOrganization.OrganizationNumber;
        _dbContext.Update(existingOrganization);
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