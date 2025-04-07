using EShop.Identity.Tests.Setups;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using EShop.Shared.Scoping;
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
            var operationUserData = GetUserPerformingAction(operationUsername, request.ParentOrganizationId);
            var result = await _apiContext.PostAsync<Command.CreateOrganizationCommand>(BaseUrl, request, operationUserData);
        }
        catch (Exception ex)
        {
            _apiContext.LastApiError = ex;
        }
    }

    private UserData GetUserPerformingAction(string? opertionalUsername, string? tenantId = null)
    {
        if (string.IsNullOrEmpty(opertionalUsername))
        {
            return _apiContext.GetUserByUsername(opertionalUsername);
        }

        var user = UserData.IsSystemUser(opertionalUsername)
            ? UserData.GetSystemUser(tenantId)
            : new UserData(opertionalUsername, opertionalUsername, tenantId ?? string.Empty);

        _apiContext.AddUser(user);
        return user;
    }
}