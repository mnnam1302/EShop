using EShop.Identity.Domain.Entities;
using EShop.Identity.Domain.Repositories;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Identity.Application.Services;

public interface IPermissionCalculator
{
    Task<string[]> CalculateFor(string userId, CancellationToken cancellationToken = default);
}

public class PermissionCalculator : IPermissionCalculator, IUserPermissionsProvider
{
    private readonly IIdentityRepositoryBase<User, string> _userRepository;
    private readonly ILogger _logger;

    public PermissionCalculator(IIdentityRepositoryBase<User, string> userRepository, ILogger<PermissionCalculator> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public Task<string[]> GetPermissions(string userId) => CalculateFor(userId.ToLower());

    public async Task<string[]> CalculateFor(string userId, CancellationToken cancellationToken = default)
    {
        var user = _userRepository.FindByCondition(x => x.Id == userId, false, x => x.Roles);

        var userPermissions = user
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
            .SelectMany(u => u.UserRoles!
                .SelectMany(ur => ur.Role!.RolePermissions!
                    .Where(rp => rp.PermissionId != null)
                    .Select(rp => rp.PermissionId!)));

        var distinctPermissions = await userPermissions.Distinct().ToArrayAsync();

        _logger.LogInformation(
            "Calculated permissions for user '{UserId}' based on roles stored in the database. Result: {Count} permissions available.",
            userId, distinctPermissions.Length);

        return distinctPermissions;
    }
}