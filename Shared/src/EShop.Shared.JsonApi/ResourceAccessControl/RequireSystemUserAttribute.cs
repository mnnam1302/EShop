using EShop.Shared.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireSystemUserAttribute : Attribute, IUserPermissionFilter
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new InternalRequireSystemUserFilter(serviceProvider.GetRequiredService<IUserDetailsProvider>());
    }

    private sealed class InternalRequireSystemUserFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserDetailsProvider userDetailsProvider;

        public InternalRequireSystemUserFilter(IUserDetailsProvider userDetailsProvider)
        {
            this.userDetailsProvider = userDetailsProvider;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }

            if (!userDetailsProvider.IsSystemUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }

            return Task.CompletedTask;
        }
    }
}