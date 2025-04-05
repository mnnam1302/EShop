using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireFeatureAttribute : Attribute, IFilterFactory
{
    public RequireFeatureAttribute(string featureId)
    {
        Feature = featureId;
    }

    public string Feature { get; set; } = string.Empty;

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
        private readonly string _requiredFeature;
        private readonly IFeatureValidator _featureValidator;
        private readonly IUserDetailsProvider _userDetailsProvider;
        private ILogger _logger;

        internal InternalRequireFeatureFilter(IFeatureValidator featureValidator, IUserDetailsProvider userDetailsProvider, ILogger logger, string requiredFeature)
        {
            _featureValidator = featureValidator;
            _userDetailsProvider = userDetailsProvider;
            _logger = logger;
            _requiredFeature = requiredFeature;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!_userDetailsProvider.IsAuthenticatedUser)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                _logger.LogTrace("Rejecting authorizated user");
                return;
            }

            if (!await _featureValidator.HasFeatureAsync(_requiredFeature))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                _logger.LogTrace("Rejecting user without {expectedFeature}", _requiredFeature);
                return;
            }
        }
    }
}