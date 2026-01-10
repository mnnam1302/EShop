using Carter;
using EShop.Shared.JsonApi.ResourceAccessControl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EShop.Tenancy.Presentation.APIs;

public sealed class FeatureApi : ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/features";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.NewVersionedApi("Features")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireAuthorization();

        group.MapGet("/", GetSystemFeaturesAsync)
            .RequireSystemUserFilter();
    }

    private static async Task<IResult> GetSystemFeaturesAsync()
    {
        return Results.Ok();
    }
}