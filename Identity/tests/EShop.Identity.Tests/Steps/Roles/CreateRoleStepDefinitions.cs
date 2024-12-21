using Reqnroll;

namespace EShop.Identity.Tests.Steps.Roles
{
    [Binding]
    public sealed class CreateRoleStepDefinitions
    {
        //public CreateRoleStepDefinitions(Role role)
        //{
        //    _role = role;
        //}

        [Given("There is a new role with the following data")]
        public void GivenThereIsANewRoleWithTheFollowingData(DataTable dataTable)
        {
            var a = 10;
        }

        [When("Create new role")]
        public void WhenCreateNewRole()
        {
        }

        [Then("A new role created with following data")]
        public void ThenANewRoleCreatedWithFollowingData(DataTable dataTable)
        {
        }
    }
}