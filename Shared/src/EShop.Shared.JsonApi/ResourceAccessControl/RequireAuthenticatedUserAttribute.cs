using EShop.Shared.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAuthenticatedUserAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new InternalRequireAuthenticatedUserFilter(serviceProvider.GetRequiredService<IUserDetailsProvider>());
    }

    private sealed class InternalRequireAuthenticatedUserFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserDetailsProvider userDetailsProvider;

        public InternalRequireAuthenticatedUserFilter(IUserDetailsProvider userDetailsProvider)
        {
            this.userDetailsProvider = userDetailsProvider;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
            return Task.CompletedTask;
        }
    }
}