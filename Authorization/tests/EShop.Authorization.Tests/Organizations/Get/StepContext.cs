using EShop.Authorization.Application.UseCases.Organizations;
using EShop.Authorization.Tests.Setups;

namespace EShop.Authorization.Tests.Organizations.Get;

internal sealed class StepContext(ApiContext apiContext)
{
    internal async Task<List<OrganizationsResponse>> GetOrganizations(string username)
    {
        var operationalUser = apiContext.GetUserByUsername(username);

        var result = await apiContext.GetAsync<List<OrganizationsResponse>>("api/v1/organizations", operationalUser);
        return result.Value;
    }
}