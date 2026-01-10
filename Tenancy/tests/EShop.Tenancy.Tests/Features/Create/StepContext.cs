using EShop.Tenancy.Application.UseCases.V1.Queries.Features;
using EShop.Tenancy.Presentation.Models;
using EShop.Tenancy.Tests.Setups;

namespace EShop.Tenancy.Tests.Features.Create;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "api/v1/features";

    internal async Task CreateSystemFeature(CreateSystemFeatureRequest request, string? operationalUsername = null)
    {
        try
        {
            var operationalUser = apiContext.GetUserByUsername(operationalUsername);

            await apiContext.PostAsync(BaseUrl, request, operationalUser);
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }

    internal async Task<FeatureResponse> GetSystemFeature(string featureId, string? operationalUsername = null)
    {
        var operationalUser = apiContext.GetUserByUsername(operationalUsername);

        var response = await apiContext.GetAsync<FeatureResponse>($"{BaseUrl}/{featureId}", operationalUser);
        return response.Value;
    }
}