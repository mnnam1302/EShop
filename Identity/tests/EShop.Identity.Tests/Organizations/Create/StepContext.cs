using EShop.Identity.Tests.Setups;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using Reqnroll;

namespace EShop.Identity.Tests.Organizations.Create;

[Binding]
public sealed class StepContext
{
    private const string BaseUrl = "/api/v1/organizations";
    private readonly ApiContext _apiContext;

    public StepContext(ApiContext apiContext)
    {
        _apiContext = apiContext;
    }

    public async Task CreateOrganizationAsync(Command.CreateOrganization request, string? operationUsername = null)
    {
        try
        {
            var result = await _apiContext.PostAsync<Command.CreateOrganization>(BaseUrl, request);
        }
        catch (Exception ex)
        {
            _apiContext.LastApiError = ex;
        }
    }

    //public async Task GetAllOrganizations(Query)
    //{

    //}
}