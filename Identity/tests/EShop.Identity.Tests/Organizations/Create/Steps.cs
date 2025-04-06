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
    public void GivenFollowingTenantsAddedToTheSystem(DataTable dataTable)
    {
        throw new PendingStepException();
    }

    [Given("Admin user creates a new organization with the following")]
    [When("Admin user creates a new organization with the following")]
    public async Task WhenAdminUserCreatesANewOrganizationWithTheFollowing(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<Command.CreateOrganizationCommand>();
        await _stepContext.CreateOrganizationAsync(request);
    }
}