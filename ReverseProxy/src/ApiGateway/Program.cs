using ApiGateway.DependencyInjections.Extensions;
using ApiGateway.Middlewares;
using EShop.Shared.Diagnostics;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Logging.SetSerilog("ApiGateway");
builder.Host.UseSerilog();

builder.Services
    .AddBoostrapping(builder.Configuration)
    .AddShared(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

app.UseCors();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
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
    Log.CloseAndFlush();
    await app.DisposeAsync();
}

public partial class Program { }