using EShop.Identity.Tests.Setups;
using EShop.Shared.Contracts.Abstractions.Shared;
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

    public async Task CreateOrganizationAsync(Command.CreateOrganizationCommand request, string? operationUsername = null)
    {
        try
        {
            var result = await _apiContext.PostAsync<Command.CreateOrganizationCommand>(BaseUrl, request);
        }
        catch (Exception ex)
        {
            _apiContext.LastApiError = ex;
        }
    }

    public async Task<Result<Response.OrganizationResponse>> GetOrganizationByIdAsync(Query.GetOrganizationById request, string? operationUsername = null)
    {
        try
        {
            var result = await _apiContext.GetAsync<Response.OrganizationResponse>(
                $"{BaseUrl}/{request.Id}");

            return result;
        }
        catch (Exception ex)
        {
            _apiContext.LastApiError = ex;
            return Result.Failure<Response.OrganizationResponse>(Error.NullValue);
        }
    }
}