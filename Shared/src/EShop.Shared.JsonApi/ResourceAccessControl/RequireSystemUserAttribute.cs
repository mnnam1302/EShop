using EShop.Shared.Scoping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireSystemUserAttribute : Attribute, IFilterFactory, IUserPermissionFilter
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new InternalRequireSystemUserFilter(serviceProvider.GetRequiredService<IUserDetailsProvider>());
    }

    private sealed class InternalRequireSystemUserFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserDetailsProvider _userDetailsProvider;

        public InternalRequireSystemUserFilter(IUserDetailsProvider userDetailsProvider)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!_userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }

            if (!_userDetailsProvider.IsSystemUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }

            return Task.CompletedTask;
        }
    }
}