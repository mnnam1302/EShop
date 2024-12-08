using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.API.Middlewares;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.DependencyInjections;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Shared.JsonApi
//builder.Services.AddUserScoping();
//builder.Services.AddMultiTenantScoping();

// Shared.Cache
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddUserTokenCachingService();

/*
 * API - Rule DI
 * - Jwt Authentication owner service
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

builder.Services.AddUserPermissionForOwnerServiceAPI();

// Application
builder.Services.AddMediatRApplication();
builder.Services.AddAutoMapperApplication();

// Persistence
//builder.Services.ConfigureNgSqlRetryOptionsPersistence(builder.Configuration.GetSection("NgSqlRetryOptions"));
//builder.Services.AddNqSqlPersistence(builder.Configuration);

builder.Services.AddDbContextWithScoping<UserDbContext>(builder.Configuration, false);
builder.Services.ConfigureServices();
builder.Services.AddRepositoryPersistence();

// Infrastructure
builder.Services.AddServicesInfrastructure();

// Middleware
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddAuthorization();
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwaggerAPI();
}

//app.UseHttpsRedirection();
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
