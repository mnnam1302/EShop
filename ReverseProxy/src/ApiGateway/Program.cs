using ApiGateway.DependencyInjections.Extensions;
using EShop.Shared.Diagnostics;
using EShop.Shared.JsonApi.Middlewares;
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

if (app.Environment.IsDevelopment())
{
    app.UseCors("CorsPolicy");
}

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
    await Log.CloseAndFlushAsync();
    await app.DisposeAsync();
}

public partial class Program { }