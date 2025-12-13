using EShop.Catalog.Application.Categories.Create;
using EShop.Catalog.Tests.Setup;

namespace EShop.Catalog.Tests.Categories.Create;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/categories";

    public async Task CreateCategoryAsync(CreateCategoryRequest request, string? operationUsername = null)
    {
        try
        {
            var operationUser = apiContext.GetUserByUsername(operationUsername);
            var response = await apiContext.PostAsync(BaseUrl, request, operationUser);

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