using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireOneOfPermissionsAttribute : Attribute, IUserPermissionFilter
{
    public RequireOneOfPermissionsAttribute(params string[] permissions)
    {
        Permissions = permissions;
    }

    public string[] Permissions { get; set; }
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
        private readonly string[] requirePermissions;
        private readonly IPermissionValidator permissionValidator;
        private readonly IUserDetailsProvider userDetailsProvider;
        private readonly ILogger logger;

        public InternalRequireOneOfPermissionFilter(
            IPermissionValidator permissionValidator,
            IUserDetailsProvider userDetailsProvider,
            ILogger<InternalRequireOneOfPermissionFilter> logger,
            string[] requirePermissions)
        {
            this.permissionValidator = permissionValidator;
            this.userDetailsProvider = userDetailsProvider;
            this.requirePermissions = requirePermissions;
            this.logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                logger.LogTrace("Rejecting unauthenticated user");
            }

            if (!await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(requirePermissions))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                logger.LogTrace("Rejecting user without {ExpectedPermission} permissions", requirePermissions.ToString());
            }
        }
    }
}