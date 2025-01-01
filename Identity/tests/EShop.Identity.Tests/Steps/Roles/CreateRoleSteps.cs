using EShop.Identity.Domain.Entities;
using EShop.Identity.Tests.Steps.StepContext;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    public void ThenThereAreFollowingRolesInTheSystem(DataTable dataTable)
    {
        var actualRoles = await _roleContext.GetAllRolesAsync();
        AssertRolesList(actualRoles, dataTable);
    }

    private void AssertRolesList(IReadOnlyCollection<Role> actualRoles, DataTable table)
    {
        actualRoles.Count.Should().Be(table.RowCount);

        actualRoles.Select(x => new { Name = x.Name, TenantId = x.TenantId })
            .Should()
            .BeEquivalentTo(table.Rows.Select(row => new { Name = row["Name"], TenantId = row["TenantId"] }));

        if (table.ContainsColumn("Description"))
        {
            actualRoles.Select(x => x.Description)
                .Should()
                .BeEquivalentTo(table.Rows.Select(row => row["Description"]));
        }
    }

}