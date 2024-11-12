using EShop.Identity.Domain.Abstractions.Repositories;
using EShop.Identity.Domain.Entities;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace EShop.Identity.Application.Services;

public interface IPermissionCalculator
{
    Task<string[]> CalculateFor(string userId, CancellationToken cancellationToken = default);
}

public class PermissionCalculator : IPermissionCalculator, IUserPermissionsProvider
{
    private readonly IRepositoryBase<User, string> _userRepository;
    private readonly ILogger<PermissionCalculator> logger;

    public PermissionCalculator(IRepositoryBase<User, string> userRepository, ILogger<PermissionCalculator> logger)
    {
        _userRepository = userRepository;
        this.logger = logger;
    }

    public async Task<string[]> CalculateFor(string userId, CancellationToken cancellationToken = default)
    {
        var user = _userRepository.FindAll(x => x.Id == userId, x => x.Roles);

        var userPermissions = user
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r!.RolePermissions)
            .SelectMany(u => u.UserRoles!
                .SelectMany(ur => ur.Role!.RolePermissions!
                    .Where(rp => rp.PermissionId != null)
                    .Select(rp => rp.PermissionId!)));

        var distinctPermissions = await userPermissions.Distinct().ToArrayAsync();
        logger.LogDebug("Calculated permissions for user '{id}' based on roles stored in the database. Result: {count} permissions available.",
            userId, distinctPermissions.Length);

        return distinctPermissions;
    }

    public Task<string[]> GetPermissions(string userId) => CalculateFor(userId.ToLower());
}