using EShop.ApiGateway.Extensions;
using EShop.Shared.Diagnostics;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Logging.SetSerilog("ApiGateway");
builder.Host.UseSerilog();

builder.Services
    .AddShared(builder.Configuration)
    .AddBoostrapping(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseCors(CorsConstants.DevelopmentCorsPolicy);
}
else
{
    app.UseCors(CorsConstants.ProductionCorsPolicy);
}

// Enable in production
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();
app.MapDefaultEndpoints();
app.MapReverseProxy();

try
{
    Log.Information("Starting up ApiGateway...");
    await app.RunAsync();
    Log.Information("Stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    await app.StopAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
    await app.DisposeAsync();
}