using EShop.Authorization.API.Models;
using EShop.Authorization.Tests.Setups;

namespace EShop.Authorization.Tests.Organizations.AddChild;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/organizations";

    internal async Task AddChildOrganizationAsync(AddChildOrganizationRequest request, string parentOrganizationId, string username)
    {
        try
        {
            var operationalUser = apiContext.GetUserByUsername(username);

            await apiContext.PostAsync(
                $"{BaseUrl}/{parentOrganizationId}/child-organizations",
                request,
                operationalUser);
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
        }
    }
}
