using EShop.Identity.Tests.Steps.StepContext;
using EShop.Shared.Contracts.Services.Identity.Roles;
using FluentAssertions;
using Reqnroll;

namespace EShop.Identity.Tests.Steps.Roles;

[Binding]
internal class CreateRoleSteps
{
    private readonly RoleContext _roleContext;

    public CreateRoleSteps(RoleContext roleContext)
    {
        _roleContext = roleContext;
    }

    [When("user '(.*)' create role '(.*)'")]
    public async Task WhenUserCreateRole(string creatorUsername, string roleName)
    {
        _roleContext.Name = roleName ?? _roleContext.Name;
        await _roleContext.CreateRoleAsync(creatorUsername);
    }

    [Then("there are following Roles in the system")]
    public async Task ThenThereAreFollowingRolesInTheSystem(DataTable dataTable)
    {
        var actualRoles = await _roleContext.GetAllRolesAsync();
        AssertRolesList(actualRoles, dataTable);
    }

    private void AssertRolesList(List<Response.RolesResponse> actualRoles, DataTable table)
    {
        actualRoles.Count.Should().Be(table.RowCount);

        //actualRoles.Select(x => new { Name = x.Name, TenantId = x.TenantId })
        //    .Should()
        //    .BeEquivalentTo(table.Rows.Select(row => new { Name = row["Name"], TenantId = row["TenantId"] }));
        
        actualRoles.Select(x => new { Name = x.Name })
            .Should()
            .BeEquivalentTo(table.Rows.Select(row => new { Name = row["Name"] }));

        if (table.ContainsColumn("Description"))
        {
            actualRoles.Select(x => x.Description)
                .Should()
                .BeEquivalentTo(table.Rows.Select(row => row["Description"]));
        }
    }
}