using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Scoping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAuthenticatedUserAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new InternalRequireAuthenticatedUserFilter(serviceProvider.GetRequiredService<IUserDetailsProvider>());
    }

    private class InternalRequireAuthenticatedUserFilter : IAsyncAuthorizationFilter
    {
        private readonly IUserDetailsProvider _userDetailsProvider;

        public InternalRequireAuthenticatedUserFilter(IUserDetailsProvider userDetailsProvider)
        {
            _userDetailsProvider = userDetailsProvider;
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!_userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            }
            return Task.CompletedTask;
        }
    }
}