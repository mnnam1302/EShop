using EShop.Shared.Contracts.Services.Identity.Organizations;
using Reqnroll;

namespace EShop.Identity.Tests.Organizations.Update;

[Binding]
public class Steps
{
    private readonly StepContext _stepContext;

    public Steps(StepContext stepContext)
    {
        _stepContext = stepContext;
    }

    [When("Admin user updates the organization {string} with the following")]
    public async Task WhenAdminUserUpdatesTheOrganizationWithTheFollowing(string organizationName, DataTable dataTable)
    {
        var request = dataTable.CreateInstance<Command.UpdateOrganizationCommand>();
        await _stepContext.UpdateOrganizationAsync(organizationName, request);
    }
}