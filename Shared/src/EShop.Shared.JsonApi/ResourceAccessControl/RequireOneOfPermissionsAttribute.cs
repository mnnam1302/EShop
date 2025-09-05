using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireOneOfPermissionsAttribute : Attribute, IUserPermissionFilter
{
    public RequireOneOfPermissionsAttribute(params string[] permissions)
    {
        Permissions = permissions;
    }

    public string[] Permissions { get; set; } = [];
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new InternalRequireOneOfPermissionFilter(
            serviceProvider.GetRequiredService<IPermissionValidator>(),
            serviceProvider.GetRequiredService<IUserDetailsProvider>(),
            serviceProvider.GetRequiredService<ILogger<InternalRequireOneOfPermissionFilter>>(),
            Permissions);
    }

    private sealed class InternalRequireOneOfPermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string[] _requirePermissions;
        private readonly IPermissionValidator _permissionValidator;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private readonly ILogger _logger;

        public InternalRequireOneOfPermissionFilter(
            IPermissionValidator permissionValidator,
            IUserDetailsProvider userDetailsProvider,
            ILogger<InternalRequireOneOfPermissionFilter> logger,
            string[] requirePermissions)
        {
            _permissionValidator = permissionValidator;
            _userDetailsProvider = userDetailsProvider;
            _requirePermissions = requirePermissions;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!_userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                _logger.LogTrace("Rejecting unauthenticated user");
            }

            if (!await _permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(_requirePermissions))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                _logger.LogTrace("Rejecting user without {ExpectedPermission} permissions", _requirePermissions.ToString());
            }
        }
    }
}