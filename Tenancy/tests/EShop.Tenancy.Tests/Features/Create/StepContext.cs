using EShop.Shared.Authentication;
using EShop.Tenancy.Application.UseCases.V1.Queries.Features;
using EShop.Tenancy.Presentation.Models;
using EShop.Tenancy.Tests.Setups;
using EShop.Testing.JsonApiApplication;

namespace EShop.Tenancy.Tests.Features.Create;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "api/v1/features";

    internal async Task CreateSystemFeature(CreateSystemFeatureRequest request)
    {
        try
        {
            var systemUser = UserData.GetSystemUser(ApiTestContextBase.DefaultTenantId);
            await apiContext.PostAsync(BaseUrl, request, systemUser);
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }

    internal async Task<FeatureResponse> GetSystemFeature(string featureId)
    {
        var systemUser = UserData.GetSystemUser(ApiTestContextBase.DefaultTenantId);
        var response = await apiContext.GetAsync<FeatureResponse>($"{BaseUrl}/{featureId}", systemUser);
        return response.Value;
    }
}