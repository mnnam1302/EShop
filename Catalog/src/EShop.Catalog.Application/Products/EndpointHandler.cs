using EShop.Catalog.Application.Products.AddVariant;
using EShop.Catalog.Application.Products.AddVariationDimension;
using EShop.Catalog.Application.Products.ChangeVariantPrice;
using EShop.Catalog.Application.Products.ChangeVariationDimensionValues;
using EShop.Catalog.Application.Products.Create;
using EShop.Catalog.Application.Products.Delete;
using EShop.Catalog.Application.Products.Publish;
using EShop.Catalog.Application.Products.PublishVariant;
using EShop.Catalog.Application.Products.Unpublish;
using EShop.Catalog.Application.Products.UnpublishVariant;
using EShop.Catalog.Application.Products.UpdateVariant;
using EShop.Catalog.Application.Products.UpdateVariationDimension;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Products;

public static class EndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/products";

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var productEndpointsV1 = routerBuilder
            .NewVersionedApi("Product")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Catalog.ProductFeatureId);

        productEndpointsV1
            .MapCreateProduct()
            .MapPublishProduct()
            .MapUnpublishProduct()
            .MapDeleteProduct()
            .MapAddVariationDimension()
            .MapUpdateVariationDimension()
            .MapChangeVariationDimensionValues()
            .MapAddVariant()
            .MapUpdateVariant()
            .MapChangeVariantPrice()
            .MapPublishVariant()
            .MapUnpublishVariant();

        return routerBuilder;
    }
}
