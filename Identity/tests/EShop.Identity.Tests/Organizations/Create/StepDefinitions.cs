using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using FluentAssertions;
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
    public async Task ThenThereAreFollowingOrganization(DataTable dataTable)
    {
        var expectedOrganization = dataTable.CreateInstance<Organization>();
        var request = new Query.GetOrganizationById(expectedOrganization.Name);

        var actualOrganization = await _stepContext.GetOrganizationByIdAsync(request);

        actualOrganization.IsSuccess.Should().BeTrue();
        dataTable.CompareToInstance(actualOrganization.Value);
    }
}