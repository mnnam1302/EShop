using EShop.Catalog.Application.Categories.Create;
using EShop.Catalog.Application.Categories.Publish;
using EShop.Catalog.Application.Categories.Unpublish;
using EShop.Catalog.Application.Categories.Update;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Categories;

public static class EndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/categories";

    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var categoryEndpointsV1 = routerBuilder
            .NewVersionedApi("Category")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Catalog.ProductFeatureId);

        categoryEndpointsV1
            .MapCreateCategory()
            .MapUpdateCategory()
            .MapPublishCategory()
            .MapUnpublishCategory();

        return routerBuilder;
    }
}
