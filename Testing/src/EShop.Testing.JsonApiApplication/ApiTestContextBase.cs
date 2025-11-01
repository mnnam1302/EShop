using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Managers;
using EShop.Shared.Authentication.Managers.JwtTokens;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Testing.JsonApiApplication.EventBus;
using EShop.Testing.JsonApiApplication.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Serilog;
using System.Net;
using System.Reflection;
using System.Text;

namespace EShop.Testing.JsonApiApplication;

public interface IApiTestContextBase
{
    IServiceProvider ServiceProvider { get; }

    Exception LastApiError { get; }

    HttpClient GetAuthorizedClient(UserData user, string acceptHeader = "application/json");

    UserData GetUserByUsername(string? username = null);
}

public abstract class ApiTestContextBase
{
    public const string DefaultTenantId = "TEST-TENANT";
    public const string DefaultOrganizationEmail = "test_organization@eshop.ecommerce";
    public const string DefaultUserEmail = "test_admin@test.com";
    public const string SourceSystem = "BddTest";

    protected static readonly string[] AllFeatureIds = typeof(FeatureIds)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.Name != nameof(FeatureIds.InitialState))
        .Select(fi => fi.GetValue(null)?.ToString())
        .Where(featureId => featureId is not null)
        .ToArray()!;

    protected static readonly string[] StandardFeatureIds = [.. AllFeatureIds];

    protected static readonly string[] AllPermissionIds = typeof(PermissionConstants)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
        .Select(fi => fi.GetValue(null)?.ToString())
        .Where(permissionId => permissionId is not null)
        .ToArray()!;
}

public abstract class ApiTestContextBase<TStartup> : ApiTestContextBase, IApiTestContextBase, IDisposable
    where TStartup : class
{
    private bool disposed = false;
    private readonly TestServer server;
    private readonly IServiceScope serviceScope;
    private readonly Microsoft.Extensions.Logging.ILogger logger;
    private readonly TestUserPermissionProvider testUserPermissionProvider;
    private readonly TestTenantFeatureProvider testTenantFeatureProvider;

    private readonly Dictionary<string, UserData> _users = [];

    private UserData defaultUser = new("TEST_ADMIN", "TEST_ADMIN", DefaultTenantId, isSupportUser: true);

    private string LoggedInUser;

    protected ApiTestContextBase(Func<WebHostBuilderContext, TStartup> startupFactory)
    {
        var webHostBuilder = new WebHostBuilder()
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<HttpClient>(sp => this.GetClientWithHeaderPropagation());
                services.AddSingleton<IIntegrationEventsTracker, IntegrationEventsTracker>();
                services.AddSerilog(SetupLogging);
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddUserSecrets<TStartup>();
            });

        webHostBuilder = startupFactory is not null
            ? webHostBuilder.UseStartup(startupFactory)
            : webHostBuilder.UseStartup<TStartup>();

        webHostBuilder = webHostBuilder.ConfigureLogging(builder => builder.ClearProviders().AddSerilog());

        server = new TestServer(webHostBuilder);
        serviceScope = server.Host.Services.CreateScope();

        logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ApiTestContextBase<TStartup>>();

        testUserPermissionProvider = ServiceProvider.GetRequiredService<IUserPermissionsProvider>() as TestUserPermissionProvider
                                     ?? throw new InvalidOperationException("Service provider did not return a TestUserPermissionProvider instance.");

        testTenantFeatureProvider = ServiceProvider.GetRequiredService<ITenantFeaturesProvider>() as TestTenantFeatureProvider
                                     ?? throw new InvalidOperationException("Service provider did not return a TestTenantFeatureProvider instance.");

        EventTracker = ServiceProvider.GetRequiredService<IIntegrationEventsTracker>();
    }

    public Microsoft.Extensions.Logging.ILogger Logger => logger;
    public ILoggerFactory LoggerFactory => ServiceProvider.GetRequiredService<ILoggerFactory>();
    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;
    public IIntegrationEventsTracker EventTracker { get; private set; }
    public Exception LastApiError { get; set; }
    public HttpClient Client => GetAuthorizedClient(defaultUser);
    public HttpStatusCode LastStatusCode { get; set; }

    #region Manage User Management

    public void AddUser(UserData user, bool setAsDefault = false)
    {
        _users.Add(user.Username.ToLower(), user);
        if (setAsDefault)
        {
            defaultUser = user;
            logger.LogWarning("Changing default user to '{username}'({tenantId})", user.Username, user.TenantId);
        }
    }

    public void AddOrUpdateUser(UserData user, bool setAsDefault = false)
    {
        if (_users.ContainsKey(user.Username))
        {
            _users[user.Username] = user;
        }
        else
        {
            _users.Add(user.Username.ToLower(), user);
        }
        if (setAsDefault)
        {
            defaultUser = user;
            logger.LogWarning("Changing default user to '{username}'({tenantId})", user.Username, user.TenantId);
        }
    }

    public UserData GetUserByUsername(string? username = null)
    {
        var operationalUsername = username ?? LoggedInUser;
        operationalUsername = operationalUsername?.ToLower();
        if (string.IsNullOrEmpty(operationalUsername) || string.Equals(username, defaultUser.Username, StringComparison.OrdinalIgnoreCase))
        {
            return defaultUser;
        }
        else
        {
            if (_users.ContainsKey(operationalUsername))
            {
                return _users[operationalUsername];
            }

            if (_users.Keys.Count(x => x.StartsWith(operationalUsername)) == 1)
            {
                return _users.Single(kv => kv.Key.StartsWith(operationalUsername)).Value;
            }

            if (!operationalUsername.Contains('@'))
            {
                operationalUsername = $"{operationalUsername}@{ApiTestContextBase.DefaultTenantId}";
            }

            return new UserData(operationalUsername, operationalUsername, ApiTestContextBase.DefaultTenantId);
        }
    }

    public bool CheckUserExists(string username)
    {
        var realUsername = GetUserByUsername(username).Username;
        return _users.ContainsKey(realUsername);
    }

    public void SetupPermissionsForDefaultAdminUser(string[] permissionIds)
    {
        var adminUser = GetUserByUsername();
        foreach (var permissionId in permissionIds)
        {
            AddPermissionToUser(adminUser.Id, permissionId);
        }
    }

    public void SetupPermissionsForUser(string username, string[] permissionIds)
    {
        var user = GetUserByUsername(username);
        foreach (var permissionId in permissionIds)
        {
            AddPermissionToUser(user.Id, permissionId);
        }
    }

    public void AddPermissionToUser(string userId, string permissionId)
    {
        testUserPermissionProvider.AddPermission(userId, permissionId);
    }

    public void GrantAllPermissionsToUser(string userId)
    {
        foreach (var permissionId in AllPermissionIds)
        {
            testUserPermissionProvider.AddPermission(userId, permissionId);
        }
    }

    public void SetupStandardFeaturesForDefaultTenant()
    {
        SetupFeaturesForTenant(defaultUser.TenantId, StandardFeatureIds);
    }

    public void SetupStandardFeaturesForTenant(string tenantId)
    {
        SetupFeaturesForTenant(tenantId, StandardFeatureIds);
    }

    public void SetupFeaturesForTenant(string tenantId, string[] featureIds)
    {
        foreach (var featureId in featureIds)
        {
            testTenantFeatureProvider.AddTenantFeature(tenantId, featureId);
        }
    }

    public void UserLogsIn(string username)
    {
        var user = GetUserByUsername(username);
        if (user == null)
        {
            throw new ArgumentException($"User '{username}' not found.");
        }

        LoggedInUser = user.Username;
        logger.LogInformation("User '{username}' logged in", LoggedInUser);
    }

    #endregion Manage User Management

    #region HTTP Client Management

    private HttpClient GetClientWithHeaderPropagation()
    {
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Accept.Clear();

        var httpContext = server.Services.GetService<IHttpContextAccessor>()?.HttpContext;
        if (httpContext != null &&
            httpContext.Request.Headers.TryGetValue("Authorization", out var values) &&
            !StringValues.IsNullOrEmpty(values))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", values.ToString());
        }

        return client;
    }

    public HttpClient GetAuthorizedClient(UserData? user, string acceptHeader = "application/json")
    {
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.ParseAdd(acceptHeader);

        if (user?.UserType is UserTypes.AppClientWithIndividualUsers or UserTypes.AppClientWithoutIndividualUsers)
        {
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.UserTypeCustomHeaderName, user.UserType);
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.TenantIdCustomHeaderName, user.TenantId);
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.UserIdCustomHeaderName, user.Id);
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.ActionUserIdCustomHeaderName, user.Username);
        }

        user ??= defaultUser;
        return SystemInternalJwtTokenFactory.AddUserContext(client, user);
    }

    #endregion HTTP Client Management

    #region Http Action Method
    public Task<Result<TResponse>> GetAsync<TResponse>(string relativeUri, UserData? user = null)
    {
        return SendAsync<Result<TResponse>>(c => c.GetAsync(relativeUri), relativeUri, user, null, "GET");
    }

    public Task<Result> PostAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
    {
        return SendAsync<Result>(
             c => c.PostAsync(relativeUri, new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")),
             relativeUri, user, request, "POST");
    }

    public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
    {
        return SendAsync<Result<TResponse>>(
            c => c.PostAsync(relativeUri, new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")),
            relativeUri, user, request, "POST");
    }

    public Task<Result> PutAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
    {
        return SendAsync<Result>(
            c => c.PutAsync(relativeUri, new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")),
            relativeUri, user, request, "PUT");
    }

    public Task<Result<TResponse>> PutAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
    {
        return SendAsync<Result<TResponse>>(
            c => c.PutAsync(relativeUri, new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")),
            relativeUri, user, request, "PUT");
    }

    public Task<Result> PatchAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
    {
        return SendAsync<Result>(
            c => c.PatchAsync(relativeUri, new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")),
            relativeUri, user, request, "PATCH");
    }

    public Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
    {
        return SendAsync<Result<TResponse>>(
            c => c.PatchAsync(relativeUri, new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")),
            relativeUri, user, request, "PATCH");
    }

    public Task<Result> DeleteAsync<TRequest>(string relativeUri, UserData? user = null)
    {

        return SendAsync<Result>(c => c.DeleteAsync(relativeUri), relativeUri, user, null, "DELETE");
    }

    private async Task<TResult> SendAsync<TResult>(
        Func<HttpClient, Task<HttpResponseMessage>> sendFunc,
        string relativeUri,
        UserData? user = null,
        object? request = null,
        string method = "")
    {
        try
        {
            ArgumentNullException.ThrowIfNull(relativeUri);
            var client = GetAuthorizedClient(user);
            if (request != null)
            {
                logger.LogInformation("Sending REQUEST as '{username}': {method} {relativeUri} {jsonBody}", user?.Username ?? defaultUser?.Username, method, relativeUri, request);
            }
            else
            {
                logger.LogInformation("Sending REQUEST as '{username}': {method} {relativeUri}", user?.Username ?? defaultUser?.Username, method, relativeUri);
            }
            using var response = await sendFunc(client);
            if (typeof(TResult) == typeof(Result))
            {
                var result = await ProcessResultResponse(response);
                return (TResult)(object)result;
            }
            else
            {
                var result = await ProcessResultResponse<TResult>(response);
                return (TResult)(object)result;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error during {method} request to {relativeUri}", method, relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    private static async Task<Result> ProcessResultResponse(HttpResponseMessage response)
    {
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Result>(responseJson);
        return result ?? new Result(false, Error.NullValue);
    }

    private static async Task<Result<TValue>> ProcessResultResponse<TValue>(HttpResponseMessage response)
    {
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Result<TValue>>(responseJson);
        return result ?? new Result<TValue>(default, false, Error.NullValue);
    }

    #endregion Http Action Method

    #region Integration Event

    public async Task PublishIntegrationEvent<TEvent>(object eventData)
        where TEvent : class, IIntegrationEvent
    {
        var eventBusGateway = ServiceProvider.GetRequiredService<IEventBusGateway>();
        await eventBusGateway.PublishAsync<TEvent>(eventData);
    }

    public async Task PublishIntegrationEvent(IIntegrationEvent @event)
    {
        var eventBus = ServiceProvider.GetRequiredService<IEventBusGateway>();
        await eventBus.PublishAsync(@event);
    }

    #endregion Integration Event

    #region Logging

    private static void SetupLogging(IServiceProvider serviceProvider, LoggerConfiguration loggerConfig)
    {
        loggerConfig
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .ReadFrom.Configuration(serviceProvider.GetRequiredService<IConfiguration>())
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] (T={ThreadId}) {Message:lj} {Properties}{NewLine}{Exception}{NewLine}")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] (T={ThreadId}) {Message:lj} {Properties}{NewLine}{Exception}{NewLine}");
    }

    #endregion Logging

    #region Dispose

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            serviceScope.Dispose();

            // the factory tracks and disposes of any clients created
            server.Dispose();
        }

        disposed = true;
    }

    #endregion Dispose
}