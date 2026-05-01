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

builder.AddProject<Projects.EShop_Order_API>("eshop-order-api");

await builder.Build().RunAsync();