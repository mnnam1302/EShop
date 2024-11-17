using ApiGateway.DependencyInjections.Extensions;
using ApiGateway.Middlewares;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.DependencyInjections;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Shared.JsonApi
builder.Services.AddUserScoping();

// Shared.Cache
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddUserTokenCachingService();

builder.Services.AddCorsApiGateway();
builder.Services.AddYarpReverseProxy(builder.Configuration);
builder.Services.AddAuthenticationApiGateway(builder.Configuration);
builder.Services.AddSingleton<ExceptionHandlingMiddleware>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

try
{
    await app.RunAsync();
    Log.Information("Stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
    await app.StopAsync();
}
finally
{
    Log.CloseAndFlush();
    await app.DisposeAsync();
}

public partial class Program
{ }