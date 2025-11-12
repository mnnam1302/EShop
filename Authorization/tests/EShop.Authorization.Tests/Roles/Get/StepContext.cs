using EShop.Authorization.Application.UseCases.Roles;
using EShop.Authorization.Tests.Setups;

namespace EShop.Authorization.Tests.Roles.Get;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "api/v1/roles";

    internal async Task<RoleResponse> GetByNameAsync(string roleName, string? operationalUsername = null)
    {
        try
        {
            var operationalUser = apiContext.GetUserByUsername(operationalUsername);
            var roles = await apiContext.GetAsync<List<RoleResponse>>(
                $"{BaseUrl}?name={roleName}",
                operationalUser);

            var role = roles.Value.FirstOrDefault(r => r.Name == roleName);
            return role ?? throw new InvalidOperationException($"Role '{roleName}' not found");
        }
        catch (Exception ex)
        {
            apiContext.LastApiError = ex;
            return null!;
        }
    }
}
