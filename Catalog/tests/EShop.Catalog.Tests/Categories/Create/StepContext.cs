using EShop.Catalog.Application.Categories.Create;
using EShop.Catalog.ReadModels.MongoDb.Infrastructure;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.Tests.Setup;
using Microsoft.Extensions.DependencyInjection;

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

    public async Task CreateChildCategoryAsync(CreateCategoryRequest request, string parentReference)
    {
        var parent = await GetCategoryAsync(parentReference);
        request.ParentId = parent.DocumentId;
        await CreateCategoryAsync(request);
    }

    public async Task<Category> GetCategoryAsync(string reference)
    {
        var repository = apiContext.ServiceProvider.GetRequiredService<IMongoRepositoryBase<Category>>();
        return await repository.FindOneAsync(c => c.Reference == reference, CancellationToken.None);
    }
}