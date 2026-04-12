using EShop.Catalog.Application.Categories.Update;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Categories.Update;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/categories";

    public async Task UpdateCategoryAsync(string categoryId, UpdateCategoryRequest request)
    {
        try
        {
            var operationUser = apiContext.GetUserByUsername(null);
            var response = await apiContext.PutAsync($"{BaseUrl}/{categoryId}", request, operationUser);

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
