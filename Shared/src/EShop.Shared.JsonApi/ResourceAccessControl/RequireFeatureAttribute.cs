using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireFeatureAttribute : Attribute, IFilterFactory
{
    public RequireFeatureAttribute(string featureId)
    {
        Feature = featureId;
    }

    public string Feature { get; set; }

    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new InternalRequireFeatureFilter(
            serviceProvider.GetRequiredService<IFeatureValidator>(),
            serviceProvider.GetRequiredService<IUserDetailsProvider>(),
            serviceProvider.GetRequiredService<ILogger<RequireFeatureAttribute>>(),
            Feature);
    }

    private sealed class InternalRequireFeatureFilter : IAsyncAuthorizationFilter
    {
        private readonly string requiredFeature;
        private readonly IFeatureValidator featureValidator;
        private readonly IUserDetailsProvider userDetailsProvider;
        private readonly ILogger logger;

        internal InternalRequireFeatureFilter(
            IFeatureValidator featureValidator,
            IUserDetailsProvider userDetailsProvider,
            ILogger logger,
            string requiredFeature)
        {
            this.featureValidator = featureValidator;
            this.userDetailsProvider = userDetailsProvider;
            this.logger = logger;
            this.requiredFeature = requiredFeature;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                logger.LogTrace("Rejecting unauthenticated user");
                return;
            }

            if (!await featureValidator.HasFeatureAsync(requiredFeature))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                logger.LogTrace("Rejecting user without {ExpectedFeature}", requiredFeature);
            }
        }
    }
}