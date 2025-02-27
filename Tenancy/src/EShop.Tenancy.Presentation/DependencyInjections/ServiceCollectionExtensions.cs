using Microsoft.AspNetCore.Routing;

namespace EShop.Tenancy.Presentation.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IEndpointRouteBuilder MapTenancyServiceEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        MapTenancyEndpoints(routerBuilder);
        MapFeatureEndpoints(routerBuilder);
        return routerBuilder;
    }

    private static void MapTenancyEndpoints(this IEndpointRouteBuilder routeBuilder)
    {

    }

    private static void MapFeatureEndpoints(this IEndpointRouteBuilder routeBuilder)
    {

    }
}