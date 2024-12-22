using EShop.Identity.Tests.Setups;
using EShop.Identity.Tests.Steps.StepContext;
using Reqnroll;

namespace EShop.Identity.Tests.Steps.Users
{
    [Binding]
    internal class CreateUserSteps
    {
        private readonly ApiContext _apiContext;
        private readonly UserContext _userContext;

        public CreateUserSteps(ApiContext apiContext, UserContext userContext)
        {
            _apiContext = apiContext;
            _userContext = userContext;
        }


        [Given("following tenants added to the system")]
        public void GivenFollowingTenantsAddedToTheSystem(DataTable dataTable)
        {
            throw new PendingStepException();
        }
    }
}