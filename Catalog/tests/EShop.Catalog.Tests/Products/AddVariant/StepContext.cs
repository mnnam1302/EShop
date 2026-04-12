using EShop.Catalog.Application.Products.AddVariant;
using EShop.Catalog.Tests.Setup;

namespace EShop.Catalog.Tests.Products.AddVariant;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string ProductBaseUrl = "/api/v1/products";

    public async Task AddVariantAsync(string productId, AddVariantRequest request)
    {
        try
        {
            var response = await apiContext.PostAsync(
                $"{ProductBaseUrl}/{productId}/variants", request);

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
