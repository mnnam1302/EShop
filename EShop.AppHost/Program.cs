using EShop.AppHost.Extensions;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var externalInfrastructureMode = builder.Configuration.GetValue("ExternalInfrastructureMode", false);
var isEnabledObservability = builder.Configuration.GetValue("IsEnableObservability", false);

if (externalInfrastructureMode)
{
    builder.AddExternalServices(isEnabledObservability);
}
else
{
    builder.AddServiceDefaults(isEnabledObservability);
}

await builder.Build().RunAsync();
