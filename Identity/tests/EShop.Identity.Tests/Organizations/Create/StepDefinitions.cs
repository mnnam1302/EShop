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

    [Given("Admin user creates a new organization with the following")]
    [When("Admin user creates a new organization with the following")]
    public async Task WhenAdminUserCreatesANewOrganizationWithTheFollowing(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<Command.CreateOrganizationCommand>();
        await _stepContext.CreateOrganizationAsync(request);
    }
}