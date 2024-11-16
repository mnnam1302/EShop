using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IFilterFactory, IUserPermissionFilter
{
    public RequirePermissionAttribute()
    {
    }

    public RequirePermissionAttribute(string permissionId)
    {
        this.Permission = permissionId;
    }

    public string Permission { get; set; } = string.Empty;
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider servicePrsovider)
    {
        return new InternalRequirePermissionFilter(
            servicePrsovider.GetRequiredService<IPermissionValidator>(),
            servicePrsovider.GetRequiredService<IUserDetailsProvider>(),
            servicePrsovider.GetRequiredService<ILogger<InternalRequirePermissionFilter>>(), // no need here
            Permission);
    }

    private sealed class InternalRequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _requirePermission;
        private readonly IPermissionValidator _permissionValidator; // get permission and validator
        private readonly IUserDetailsProvider _userDetailsProvider; // contain info authenticated user
        private readonly ILogger _logger;

        public InternalRequirePermissionFilter(
            IPermissionValidator permissionValidator,
            IUserDetailsProvider userDetailsProvider,
            ILogger<InternalRequirePermissionFilter> logger,
            string requirePermission)
        {
            _permissionValidator = permissionValidator;
            _userDetailsProvider = userDetailsProvider;
            _requirePermission = requirePermission;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!_userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                _logger.LogTrace("Rejecting authorizated user");
                return;
            }

            if (!await _permissionValidator.HasPermissionAsync(_requirePermission))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                _logger.LogTrace("Rejecting user without {expectedPermission} permissions", _requirePermission);
                return;
            }
        }
    }
}