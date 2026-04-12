using EShop.Catalog.Application.Products.Create;
using EShop.Catalog.ReadModels.MongoDb.Models;
using EShop.Catalog.Tests.Setup;
using EShop.Shared.DomainTools.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Products.Create;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string ProductBaseUrl = "/api/v1/products";

    public async Task CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var operationUser = apiContext.GetUserByUsername(null);
            var response = await apiContext.PostAsync(ProductBaseUrl, request, operationUser);

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