using EShop.Tenancy.Tests.Setups;
using FluentAssertions;
using Reqnroll;
using System.Net;

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

    [Then("the system responds with status {string}")]
    public void ThenTheSystemRespondsWithStatus(string statusCode)
    {
        apiContext.LastStatusCode.Should().Be(Enum.Parse<HttpStatusCode>(statusCode));
    }
}
