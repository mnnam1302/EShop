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

        [Given("following tenant users added to the system")]
        public void GivenFollowingTenantUsersAddedToTheSystem(DataTable dataTable)
        {
            foreach (var tenant in dataTable.Rows)
            {
                _userContext.SimulateTenantCreationAsync(
                    tenant["TenantName"],
                    tenant["Username"],
                    tenant["DisplayName"],
                    tenant["Email"],
                    tenant.ContainsKey("Group") ? tenant["Group"] : string.Empty,
                    !tenant.ContainsKey("SetAsDefault") || bool.Parse(tenant["SetAsDefault"]));
            }
        }
    }
}