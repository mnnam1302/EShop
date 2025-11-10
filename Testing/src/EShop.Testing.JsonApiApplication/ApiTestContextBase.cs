using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
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

    Task<HttpClient> GetAuthorizedClient(UserData user, string acceptHeader = "application/json");

    UserData GetUserByUsername(string? username = null);
}

public abstract class ApiTestContextBase
{
    public const string DefaultTenantId = "TEST-TENANT";
    public const string DefaultOrganizationEmail = "test_organization@eshop.ecommerce";
    public const string DefaultUserEmail = "test_admin@test.com";
    public const string SourceSystem = "BddTest";

    protected static readonly string[] AllFeatureIds = typeof(FeatureConstants)
        .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
        .SelectMany(nestedType => nestedType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
            .Select(fi => fi.GetValue(null)?.ToString())
            .Where(featureId => featureId is not null))
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
    private const string JsonMediaType = "application/json";

    private bool disposed = false;
    private readonly TestServer server;
    private readonly IServiceScope serviceScope;
    private readonly Microsoft.Extensions.Logging.ILogger logger;
    private readonly TestUserPermissionProvider testUserPermissionProvider;
    private readonly TestTenantFeatureProvider testTenantFeatureProvider;
    private readonly ISystemInternalJwtTokenFactory systemInternalJwtTokenFactory;

    private readonly Dictionary<string, UserData> users = [];

    private UserData defaultUser = new("TEST_ADMIN", "TEST_ADMIN", DefaultTenantId, isSupportUser: true);

    private string LoggedInUser = string.Empty;

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

        systemInternalJwtTokenFactory = ServiceProvider.GetRequiredService<ISystemInternalJwtTokenFactory>();

        EventTracker = ServiceProvider.GetRequiredService<IIntegrationEventsTracker>();
    }

    public Microsoft.Extensions.Logging.ILogger Logger => logger;
    public ILoggerFactory LoggerFactory => ServiceProvider.GetRequiredService<ILoggerFactory>();
    public IServiceProvider ServiceProvider => serviceScope.ServiceProvider;
    public IIntegrationEventsTracker EventTracker { get; private set; }
    public Exception LastApiError { get; set; }
    public HttpStatusCode LastStatusCode { get; set; }

    #region Manage User Management

    public void AddUser(UserData user, bool setAsDefault = false)
    {
        users.Add(user.Username.ToLower(), user);
        if (setAsDefault)
        {
            defaultUser = user;
            logger.LogWarning("Changing default user to '{username}'({tenantId})", user.Username, user.TenantId);
        }
    }

    public void AddOrUpdateUser(UserData user, bool setAsDefault = false)
    {
        if (users.ContainsKey(user.Username))
        {
            users[user.Username] = user;
        }
        else
        {
            users.Add(user.Username.ToLower(), user);
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
            if (users.TryGetValue(operationalUsername, out UserData? value))
            {
                return value;
            }

            if (users.Keys.Count(x => x.StartsWith(operationalUsername)) == 1)
            {
                return users.Single(kv => kv.Key.StartsWith(operationalUsername)).Value;
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
        return users.ContainsKey(realUsername);
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

    public void SignIn(string username)
    {
        var user = GetUserByUsername(username);
        if (user == null)
        {
            throw new ArgumentException($"User '{username}' is not found.");
        }

        LoggedInUser = user.Username;
        logger.LogInformation("User '{username}' has logged in", LoggedInUser);
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

    public async Task<HttpClient> GetAuthorizedClient(UserData? user, string acceptHeader = "application/json")
    {
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.ParseAdd(acceptHeader);

        user ??= defaultUser;
        return await systemInternalJwtTokenFactory.AddUserContext(client, user);
    }

    #endregion HTTP Client Management

    #region Http Action Methods

    public Task<Result<TResponse>> GetAsync<TResponse>(string relativeUri, UserData? user = null)
    {
        return ExecuteHttpRequestAsync<Result<TResponse>>(
            client => client.GetAsync(relativeUri),
            relativeUri,
            user,
            HttpMethod.Get.Method);
    }

    public Task<Result> PostAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
    {
        return ExecuteHttpRequestWithBodyAsync<TRequest, Result>(
            relativeUri,
            request,
            user,
            HttpMethod.Post.Method);
    }

    public Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
    {
        return ExecuteHttpRequestWithBodyAsync<TRequest, Result<TResponse>>(
            relativeUri,
            request,
            user,
            HttpMethod.Post.Method);
    }

    public Task<Result> PutAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
    {
        return ExecuteHttpRequestWithBodyAsync<TRequest, Result>(
            relativeUri,
            request,
            user,
            HttpMethod.Put.Method);
    }

    public Task<Result<TResponse>> PutAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
    {
        return ExecuteHttpRequestWithBodyAsync<TRequest, Result<TResponse>>(
            relativeUri,
            request,
            user,
            HttpMethod.Put.Method);
    }

    public Task<Result> PatchAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
    {
        return ExecuteHttpRequestWithBodyAsync<TRequest, Result>(
            relativeUri,
            request,
            user,
            HttpMethod.Patch.Method);
    }

    public Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
    {
        return ExecuteHttpRequestWithBodyAsync<TRequest, Result<TResponse>>(
            relativeUri,
            request,
            user,
            HttpMethod.Patch.Method);
    }

    public Task<Result> DeleteAsync(string relativeUri, UserData? user = null)
    {
        return ExecuteHttpRequestAsync<Result>(
            client => client.DeleteAsync(relativeUri),
            relativeUri,
            user,
            HttpMethod.Delete.Method);
    }

    private Task<TResult> ExecuteHttpRequestWithBodyAsync<TRequest, TResult>(
        string relativeUri,
        TRequest request,
        UserData? user,
        string httpMethod)
    {
        var serializedRequest = System.Text.Json.JsonSerializer.Serialize(request);
        var httpContent = new StringContent(serializedRequest, Encoding.UTF8, JsonMediaType);

        return ExecuteHttpRequestAsync<TResult>(
            client => CreateHttpRequestWithBody(client, relativeUri, httpContent, httpMethod),
            relativeUri,
            user,
            httpMethod,
            request);
    }

    private static Task<HttpResponseMessage> CreateHttpRequestWithBody(HttpClient client, string relativeUri, HttpContent content, string method)
    {
        return method.ToUpperInvariant() switch
        {
            "POST" => client.PostAsync(relativeUri, content),
            "PUT" => client.PutAsync(relativeUri, content),
            "PATCH" => client.PatchAsync(relativeUri, content),
            _ => throw new ArgumentException($"HTTP method '{method}' with body is not supported.", nameof(method))
        };
    }

    private async Task<TResult> ExecuteHttpRequestAsync<TResult>(
        Func<HttpClient, Task<HttpResponseMessage>> httpRequestFactory,
        string relativeUri,
        UserData? user = null,
        string httpMethod = "",
        object? requestBody = null)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(relativeUri);

            var client = await GetAuthorizedClient(user);
            var operationalUser = user ?? defaultUser;

            if (requestBody != null)
            {
                logger.LogInformation("Sending {HttpMethod} request to {RelativeUri} as '{Username}' with body: {RequestBody}",
                    httpMethod, relativeUri, operationalUser.Username, requestBody);
            }
            else
            {
                logger.LogInformation("Sending {HttpMethod} request to {RelativeUri} as '{Username}'",
                    httpMethod, relativeUri, operationalUser.Username);
            }

            using var response = await httpRequestFactory(client);
            return await ProcessHttpResponse<TResult>(response);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error during {HttpMethod} request to {RelativeUri}", httpMethod, relativeUri);
            LastApiError = ex;
            throw;
        }
    }

    private static async Task<TResult> ProcessHttpResponse<TResult>(HttpResponseMessage response)
    {
        if (typeof(TResult) == typeof(Result))
        {
            var result = await DeserializeResultResponse(response);
            return (TResult)(object)result;
        }

        var genericResult = await DeserializeGenericResponse<TResult>(response);
        return genericResult;
    }

    private static async Task<Result> DeserializeResultResponse(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<Result>(responseContent);
        return result ?? throw new InvalidOperationException("Failed to deserialize API response to Result.");
    }

    private static async Task<TValue> DeserializeGenericResponse<TValue>(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<TValue>(responseContent);
        return result ?? throw new InvalidOperationException($"Failed to deserialize API response to {typeof(TValue).Name}.");
    }

    #endregion Http Action Methods

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

    ~ApiTestContextBase()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            serviceScope.Dispose();
            server.Dispose();
        }

        disposed = true;
    }

    #endregion Dispose
}