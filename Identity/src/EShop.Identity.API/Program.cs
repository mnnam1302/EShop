using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.API.Middlewares;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.JsonApi.DependencyInjections;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Shared: include interface and implementation
builder.Services.AddUserScoping();

/*
 * Rule: DI at API layer
 * - Jwt Authentication
 * - Swagger
 * - Interface from Shared and implemented in owner service
 */
builder.Services.AddControllers();

Log.Logger = new LoggerConfiguration().ReadFrom
    .Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging
    .ClearProviders()
    .AddSerilog();

builder.Host.UseSerilog();
builder.Services.AddControllers();

builder.Services
    .AddSwaggerGenNewtonsoftSupport()
    .AddFluentValidationRulesToSwagger()
    .AddEndpointsApiExplorer()
    .AddSwaggerAPI();

builder.Services
    .AddApiVersioning(options => options.ReportApiVersions = true)
    .AddApiExplorer(options =>
    {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddTokenCachingServiceForOwnerServiceAPI();
builder.Services.AddUserPermissionForOwnerServiceAPI();

// Application
builder.Services.AddMediatRApplication();
builder.Services.AddAutoMapperApplication();
builder.Services.AddServicesApplication();

// Persistence
builder.Services.ConfigureNgSqlRetryOptionsPersistence(builder.Configuration.GetSection("NgSqlRetryOptions"));
builder.Services.AddNqSqlPersistence(builder.Configuration);
builder.Services.ConfigureServices();
builder.Services.AddRepositoryPersistence();

// Infrastructure
builder.Services.AddServicesInfrastructure();
builder.Services.AddRedisCachingInfrastructure(builder.Configuration);

// Middleware
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
app.UseSwaggerAPI();
}

app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();

try
{
    app.ApplyMigrations();
    await app.RunAsync();
    Log.Information("Stop cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
    await app.DisposeAsync();
}

public partial class Program { }
