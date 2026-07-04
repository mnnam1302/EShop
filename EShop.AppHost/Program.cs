using EShop.AppHost.Extensions;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var useExternalInfrastructureMode = builder.Configuration.GetValue("ExternalInfrastructureMode", false);
var useExternalObservability = builder.Configuration.GetValue("ExternalObservabilityMode", false);

if (useExternalInfrastructureMode)
{
    builder.AddExternalServices(useExternalObservability);
}
else
{
    builder.AddServiceDefaults(useExternalObservability);
}

await builder.Build().RunAsync();
