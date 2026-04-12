using EShop.Catalog.Application.Products.ChangeVariantPrice;
using EShop.Catalog.Tests.Setup;

namespace EShop.Catalog.Tests.Products.ChangeVariantPrice;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string ProductBaseUrl = "/api/v1/products";

    public async Task ChangeVariantPriceAsync(string productId, string variantId, ChangeVariantPriceRequest request)
    {
        try
        {
            var response = await apiContext.PutAsync(
                $"{ProductBaseUrl}/{productId}/variants/{variantId}/price", request);

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
