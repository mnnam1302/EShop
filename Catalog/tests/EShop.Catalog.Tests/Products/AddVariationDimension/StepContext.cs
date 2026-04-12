using EShop.Catalog.Application.Products.AddVariationDimension;
using EShop.Catalog.Tests.Setup;

namespace EShop.Catalog.Tests.Products.AddVariationDimension;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string ProductBaseUrl = "/api/v1/products";

    public async Task AddVariationDimensionAsync(Guid productId, AddVariationDimensionRequest request)
    {
        try
        {
            var response = await apiContext.PostAsync(
                $"{ProductBaseUrl}/{productId}/variation-dimensions", request);

            if (response.IsFailure)
            {
                apiContext.LastApiError = new Exception(response.Error.Message);
            }
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }
}
