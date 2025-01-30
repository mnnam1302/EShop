using EShop.Identity.Tests.Setups;
using EShop.Shared.Contracts.Services.Identity.Organizations;

namespace EShop.Identity.Tests.Organizations.Update;

public class StepContext
{
    private const string BaseUrl = "/api/v1/organizations";
    private readonly ApiContext _apiContext;

    public StepContext(ApiContext apiContext)
    {
        _apiContext = apiContext;
    }

    public async Task UpdateOrganizationAsync(
        string organizationName, 
        Command.UpdateOrganizationCommand request, 
        string? operationUsername = null)
    {
        try
        {
            var result = await _apiContext.PutAsync($"{BaseUrl}/{organizationName}", request);
        }
        catch (Exception ex)
        {
            _apiContext.LastApiError = ex;
        }
    }
}