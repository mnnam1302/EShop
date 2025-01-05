using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireSupportUserAttribute : Attribute, IFilterFactory, IUserPermissionFilter
{
    public bool IsReusable => throw new NotImplementedException();

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    private sealed class InternalRequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionValidator _permissionValidator;

        public InternalRequirePermissionFilter(IPermissionValidator permissionValidator)
        {
            _permissionValidator = permissionValidator;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!await _permissionValidator.HasSupportUserAccessAsync())
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }
    }
}