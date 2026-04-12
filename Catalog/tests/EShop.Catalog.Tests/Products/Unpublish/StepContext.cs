using EShop.Catalog.Tests.Setup;

namespace EShop.Catalog.Tests.Products.Unpublish;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string ProductBaseUrl = "/api/v1/products";

    public async Task UnpublishProductAsync(Guid productId)
    {
        try
        {
            var response = await apiContext.PostAsync($"{ProductBaseUrl}/{productId}/unpublish", new { });

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
