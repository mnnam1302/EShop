
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

    public async Task SimulateTenantCreationAsync(string tenantId, string tenantName, string ownerUsername, string ownerDisplayName, string ownerEmail)
    {
        await _apiContext.PublishIntegrationEvent<Shared.Contracts.Services.Tenancy.Tenants.TenantCreated>(new
        {
            TenantId = tenantId,
            TenantName = tenantName,
            OwnerUsername = ownerUsername,
            OwnerDisplayName = ownerDisplayName,
            OwnerEmail = ownerEmail
        });
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
}