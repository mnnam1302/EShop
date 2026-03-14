using EShop.Tenancy.Tests.Setups;
using FluentAssertions;
using Reqnroll;

namespace EShop.Tenancy.Tests.Shared;

[Binding]
internal class Steps(ApiContext apiContext)
{
    [Then("the system raise an error with message {string}")]
    public void ThenTheSystemRaiseAnErrorWithMessage(string errorMessage)
    {
        apiContext.LastApiError.Should().NotBeNull();
        apiContext.LastApiError!.Message.Should().Contain(errorMessage);
    }
}
