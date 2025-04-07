using EShop.Identity.Tests.Setups;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Organizations;

namespace EShop.Identity.Tests.Organizations.Get;

public class StepContext
{
    private const string BaseUrl = "/api/v1/organizations";
    private readonly ApiContext _apiContext;

    public StepContext(ApiContext apiContext)
    {
        _apiContext = apiContext;
    }

    public async Task<Result<Response.OrganizationResponse>> GetOrganizationByIdAsync(Query.GetOrganizationById request, string? operationUsername = null)
    {
        try
        {
            var operationUserData = _apiContext.GetUserByUsername(operationUsername);
            var result = await _apiContext.GetAsync<Response.OrganizationResponse>($"{BaseUrl}/{request.Id}", operationUserData);
            return result;
        }
        catch (Exception ex)
        {
            _apiContext.LastApiError = ex;
            return Result.Failure<Response.OrganizationResponse>(Error.NullValue);
        }
    }
}