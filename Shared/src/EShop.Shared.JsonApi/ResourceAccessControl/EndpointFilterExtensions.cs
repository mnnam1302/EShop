using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.JsonApi.ResourceAccessControl;

public static class EndpointFilterExtensions
{
    public static TBuilder RequireAuthenticatedUser<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilterFactory((_, next) =>
        {
            return async invocationContext =>
            {
                var serviceProvider = invocationContext.HttpContext.RequestServices;
                var userDetailsProvider = serviceProvider.GetRequiredService<IUserDetailsProvider>();
                var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AuthenticatedUserFilterMinimalApi");

                if (!userDetailsProvider.IsAuthenticatedUser)
                {
                    logger.LogTrace("Rejecting unauthenticated user");
                    return TypedResults.Unauthorized();
                }

                return await next(invocationContext);
            };
        });

        return builder;
    }

    public static TBuilder RequireSystemUserFilter<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilterFactory((_, next) =>
        {
            return async invocationContext =>
            {
                var serviceProvider = invocationContext.HttpContext.RequestServices;
                var userDetailsProvider = serviceProvider.GetRequiredService<IUserDetailsProvider>();
                var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SystemUserFilterMinimalApi");

                if (!userDetailsProvider.IsAuthenticatedUser)
                {
                    logger.LogTrace("Rejecting unauthenticated user");
                }

                if (!userDetailsProvider.IsSystemUser)
                {
                    logger.LogTrace("Rejecting user without system user");
                    return TypedResults.Forbid();
                }

                return await next(invocationContext);
            };
        });

        return builder;
    }

    public static TBuilder RequireSupportUserFilter<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilterFactory((_, next) =>
        {
            return async invocationContext =>
            {
                var serviceProvider = invocationContext.HttpContext.RequestServices;
                var permissionValidator = serviceProvider.GetRequiredService<IPermissionValidator>();
                var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SupportUserFilterMinimalApi");

                if (!await permissionValidator.HasSupportUserAccessAsync())
                {
                    logger.LogTrace("Rejecting user without support user");
                    return TypedResults.Forbid();
                }

                return await next(invocationContext);
            };
        });

        return builder;
    }

    public static TBuilder RequirePermissionFilter<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireOneOfPermissionsFilter(permission);
    }

    public static TBuilder RequireOneOfPermissionsFilter<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilterFactory((_, next) =>
        {
            return async invocationContext =>
            {
                var serviceProvider = invocationContext.HttpContext.RequestServices;
                var userDetailsProvider = serviceProvider.GetRequiredService<IUserDetailsProvider>();
                var permissionValidator = serviceProvider.GetRequiredService<IPermissionValidator>();
                var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PermissionFilterForMinimalApi");

                if (!userDetailsProvider.IsAuthenticatedUser)
                {
                    logger.LogTrace("Rejecting unauthenticated user");
                    return TypedResults.Unauthorized();
                }

                if (await permissionValidator.HasAtLeastOneOfSpecificPermissionAsync(permissions))
                {
                    return await next(invocationContext);
                }

                if (permissions.Length == 1)
                {
                    logger.LogTrace("Rejecting user without {RequiredPermission} permission", permissions[0]);
                }
                else
                {
                    logger.LogTrace("Rejecting user without any of these permissions: {RequiredPermissions}", permissions.ToCommaSeparatedString());
                }

                return TypedResults.Forbid();
            };
        });

        return builder;
    }

    public static TBuilder RequireFeatureFilter<TBuilder>(this TBuilder builder, string feature)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireOneOfFeaturesFilter(feature);
    }

    public static TBuilder RequireOneOfFeaturesFilter<TBuilder>(this TBuilder builder, params string[] features)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilterFactory((_, next) =>
        {
            return async invocationContext =>
            {
                var serviceProvider = invocationContext.HttpContext.RequestServices;
                var userDetailsProvider = serviceProvider.GetRequiredService<IUserDetailsProvider>();
                var featureValidator = serviceProvider.GetRequiredService<IFeatureValidator>();
                var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("FeatureFilterForMinimalApi");

                if (!userDetailsProvider.IsAuthenticatedUser)
                {
                    logger.LogTrace("Rejecting unauthenticated user");
                    return TypedResults.Unauthorized();
                }

                if (await featureValidator.HasAtLeastOneOfSpecifiedFeaturesEnabledAsync(features))
                {
                    return await next(invocationContext);
                }

                if (features.Length == 1)
                {
                    logger.LogTrace("Request is blocked because feature {RequiredFeature} is not enabled", features[0]);
                }
                else
                {
                    logger.LogTrace("Request is blocked because none of these features are enabled: {RequiredFeatures}", features.ToCommaSeparatedString());
                }

                return TypedResults.Forbid();
            };
        });

        return builder;
    }
}