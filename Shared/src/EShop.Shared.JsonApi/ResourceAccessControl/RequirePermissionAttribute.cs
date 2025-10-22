using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequirePermissionAttribute : Attribute, IFilterFactory, IUserPermissionFilter
{
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
            servicePrsovider.GetRequiredService<ILogger<InternalRequirePermissionFilter>>(),
            Permission);
    }

    private sealed class InternalRequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string requirePermission;
        private readonly IPermissionValidator permissionValidator;
        private readonly IUserDetailsProvider userDetailsProvider;
        private readonly ILogger logger;

        public InternalRequirePermissionFilter(
            IPermissionValidator permissionValidator,
            IUserDetailsProvider userDetailsProvider,
            ILogger<InternalRequirePermissionFilter> logger,
            string requirePermission)
        {
            this.permissionValidator = permissionValidator;
            this.userDetailsProvider = userDetailsProvider;
            this.requirePermission = requirePermission;
            this.logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                logger.LogTrace("Rejecting unauthenticated user");
                return;
            }

            if (!await permissionValidator.HasPermissionAsync(requirePermission))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                logger.LogTrace("Rejecting user without {ExpectedPermission} permissions", requirePermission);
            }
        }
    }
}