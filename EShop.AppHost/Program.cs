using EShop.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

if (builder.Configuration["ExternalServiceMode"] == "External")
{
    builder.AddExternalServices();
}
else
{
    builder.AddServiceDefaults();
}

builder.AddProject<Projects.EShop_Finance_API>("eshop-finance-api");

await builder.Build().RunAsync();