using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.API.Middlewares;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// API
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

// Application
builder.Services.AddMediatRApplication();

// Persistence
builder.Services.ConfigureNgSqlRetryOptionsPersistence(builder.Configuration.GetSection("NgSqlRetryOptions"));
builder.Services.AddNqSqlPersistence(builder.Configuration);
builder.Services.ConfigureServices();
builder.Services.AddRepositoryPersistence();

// Infrastructure
builder.Services.AddServicesInfrastructure();

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
