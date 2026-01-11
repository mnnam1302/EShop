using EShop.Tenancy.Presentation.Models;
using Reqnroll;
using System.Threading.Tasks;

namespace EShop.Tenancy.Tests.Features.Create;

[Binding]
internal sealed class Steps(StepContext stepContext)
{
    [When("System User creates a new system feature with following details")]
    public async Task WhenSystemUserCreatesANewSystemFeatureWithFollowingDetails(DataTable dataTable)
    {
        var request = dataTable.CreateInstance<CreateSystemFeatureRequest>();
        await stepContext.CreateSystemFeature(request);
    }

    [Then("the system feature {string} has following details")]
    public async Task ThenTheSystemFeatureHasFollowingDetails(string featureId, DataTable dataTable)
    {
        var feature = await stepContext.GetSystemFeature(featureId);
        dataTable.CompareToInstance(feature);
    }
}