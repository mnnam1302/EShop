using EShop.Shared.Contracts.Services.Identity.Organizations;
using Reqnroll;

namespace EShop.Identity.Tests.Organizations.Create;

[Binding]
public class StepDefinitions
{
    private readonly StepContext _stepContext;

    public StepDefinitions(StepContext stepContext)
    {
        _stepContext = stepContext;
    }

    [When("Admin user creates a new organization with the following")]
    public async Task WhenAdminUserCreatesANewOrganizationWithTheFollowing(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<Command.CreateOrganization>();
        await _stepContext.CreateOrganizationAsync(request);
    }


    [Then("there are following organization")]
    public void ThenThereAreFollowingOrganization(DataTable dataTable)
    {
        //var actualOrganizations = _stepContext.GetAllOrganizations();
    }
}