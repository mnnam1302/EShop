using EShop.AppHost.Bootstrapping;

var builder = DistributedApplication.CreateBuilder(args);

if (builder.Configuration["ExternalServiceMode"] == "External")
{
    builder.AddExternalServices();
}
else
{
    builder.AddServiceDefaults();
}

await builder.Build().RunAsync();