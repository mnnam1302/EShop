using ApiGateway.DependencyInjections.Extensions;
using ApiGateway.DependencyInjections.Options;
using ApiGateway.Middlewares;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.JsonApi.DependencyInjections;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Shared.JsonApi
builder.Services.AddUserScoping();

// Shared.Cache
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddUserTokenCachingService();

// ApiGatewat
builder.Services.AddCorsApiGateway();
builder.Services.AddServiceDiscoveryApiGateway();
builder.Services.AddYarpReverseProxy(builder.Configuration);
builder.Services.AddAuthenticationApiGateway(builder.Configuration);
builder.Services.AddSingleton<ExceptionHandlingMiddleware>();

builder.Services.AddIdentityHttpClientOptions(builder.Configuration.GetSection(nameof(IdentityHttpClientOptions)));
builder.Services.AddUserApiHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Temp using Service Discovery here - Refactor into foler like project BFF, maybe more apply GRPC
app.MapPost("identity-api/v1/auth/login", async ([FromBody] Query.Login request, IHttpClientFactory factory) =>
{
    using var client = factory.CreateClient("UserService");

    var response = await client.PostAsJsonAsync("api/v1/auth/login", request);
    response.EnsureSuccessStatusCode();

    var jsonResponse = await response.Content.ReadFromJsonAsync<Result<Response.AuthenticatedResponse>>();
    return jsonResponse;
});

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