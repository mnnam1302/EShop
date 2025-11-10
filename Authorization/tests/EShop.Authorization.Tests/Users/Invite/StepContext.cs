using EShop.Authorization.API.Models;
using EShop.Authorization.Tests.Setups;

namespace EShop.Authorization.Tests.Users.Invite;

internal sealed class StepContext(ApiContext apiContext)
{
    private const string BaseUrl = "/api/v1/users";

    internal async Task InviteUserAsync(InviteUserRequest request, string? operationalUsername = null)
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
}
