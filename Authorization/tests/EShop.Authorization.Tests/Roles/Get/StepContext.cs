using EShop.Authorization.Application.UseCases.Roles;
using EShop.Authorization.Tests.Setups;

namespace EShop.Authorization.Tests.Roles.Get;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "api/v1/roles";

    internal async Task<RoleDetailsResponse> GetByNameAsync(string roleName, string? operationalUsername = null)
    {
        try
        {
            var operationalUser = apiContext.GetUserByUsername(operationalUsername);
            var role = await apiContext.GetAsync<RoleDetailsResponse>(
                $"{BaseUrl}?name={roleName}",
                operationalUser);

            return role.Value;
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
            return null!;
        }
    }
}
