using EShop.Identity.Domain.Entities;
using EShop.Shared.Contracts.Services.Identity.Organizations;
using FluentAssertions;
using Reqnroll;

namespace EShop.Identity.Tests.Organizations.Get;

[Binding]
public class Steps
{
    private readonly StepContext _stepContext;

    public Steps(StepContext stepContext)
    {
        _stepContext = stepContext;
    }

    [Then("there are following organization")]
    public async Task ThenThereAreFollowingOrganization(DataTable dataTable)
    {
        var expectedOrganization = dataTable.CreateInstance<Organization>();
        var request = new Query.GetOrganizationById(expectedOrganization.Name);

        var actualOrganization = await _stepContext.GetOrganizationByIdAsync(request);

        //actualOrganization.IsSuccess.Should().BeTrue();
        //dataTable.CompareToInstance(actualOrganization.Value);
    }

    [Then("organization {string} has the following details")]
    public async Task ThenOrganizationHasTheFollowingDetails(string organizationName, DataTable dataTable)
    {
        var request = new Query.GetOrganizationById(organizationName);

        var actualOrganization = await _stepContext.GetOrganizationByIdAsync(request);

        actualOrganization.IsSuccess.Should().BeTrue();
        dataTable.CompareToInstance(actualOrganization.Value);
    }
}