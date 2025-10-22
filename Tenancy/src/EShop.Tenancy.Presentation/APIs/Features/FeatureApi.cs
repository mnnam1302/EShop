using Carter;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EShop.Tenancy.Presentation.APIs.Features
{

    public sealed class FeatureApi : ICarterModule
    {
        private const string BaseUrl = "api/v{version:apiVersion}/features";

        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.NewVersionedApi("Features")
                .MapGroup(BaseUrl)
                .HasApiVersion(1)
                .RequireAuthorization();

            group.MapGet("/", GetFeaturesV1Async)
                .RequireSystemUserFilter();
        }

        private static async Task<IResult> GetFeaturesV1Async(ISender sender)
        {
            var result = await sender.Send(new Query.GetFeaturesQuery());

            if (result.IsFailure)
            {
                return ApiResultHandler.HandleFailure(result);
            }

            return Results.Ok(result);
        }
    }
}