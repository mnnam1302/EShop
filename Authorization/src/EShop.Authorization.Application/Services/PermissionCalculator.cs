using EShop.Authorization.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Authorization.Application.Services;

public interface IPermissionCalculator
{
    Task<string[]> CalculateFor(string userId, CancellationToken cancellationToken = default);
}

internal sealed class PermissionCalculator : IPermissionCalculator
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PermissionCalculator> _logger;

    public PermissionCalculator(IUserRepository userRepository, ILogger<PermissionCalculator> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<string[]> CalculateFor(string userId, CancellationToken cancellationToken = default)
    {
        var user = _userRepository.FindByCondition(x => x.Id == userId, false, x => x.Roles);

        var userPermissions = user
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
            .SelectMany(u => u.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions
                    .Where(rp => rp.PermissionId != null)
                    .Select(rp => rp.PermissionId)));

        var distinctPermissions = await userPermissions.Distinct().ToArrayAsync(cancellationToken);

        _logger.LogDebug("Calculated permissions for user '{UserId}' based on roles stored in the database. Result: {Count} permissions available.", userId, distinctPermissions.Length);

        return distinctPermissions;
    }
}
