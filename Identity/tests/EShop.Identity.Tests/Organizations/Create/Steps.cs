using EShop.Shared.Contracts.Services.Identity.Organizations;
using Reqnroll;

namespace EShop.Identity.Tests.Organizations.Create;

[Binding]
public class Steps
{
    private readonly StepContext _stepContext;

    public Steps(StepContext stepContext)
    {
        _stepContext = stepContext;
    }

    [Given("following tenants added to the system")]
    public async Task GivenFollowingTenantsAddedToTheSystem(DataTable dataTable)
    {
        var tenants = dataTable.CreateSet<Command.CreateTenantCommandInternal>();
        foreach (var tenant in tenants)
        {
            await _stepContext.SimulateTenantCreationAsync(
                tenant.TenantId,
                tenant.TenantName,
                tenant.OwnerUsername,
                tenant.OwnerDisplayName,
                tenant.OwnerEmail);
        }
    }

    [Given("Admin user creates a new organization with the following")]
    [When("Admin user creates a new organization with the following")]
    public async Task WhenAdminUserCreatesANewOrganizationWithTheFollowing(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<Command.CreateOrganizationCommand>();
        await _stepContext.CreateOrganizationAsync(request);
    }
}