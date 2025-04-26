using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireSupportUserAttribute : Attribute, IFilterFactory, IUserPermissionFilter
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new InternaSupportUserFilter(serviceProvider.GetRequiredService<IPermissionValidator>());
    }

    private sealed class InternaSupportUserFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionValidator _permissionValidator;

        public InternaSupportUserFilter(IPermissionValidator permissionValidator)
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