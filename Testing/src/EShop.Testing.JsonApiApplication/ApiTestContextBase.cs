using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
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
    public const string DefaultOrganizationEmail = "organization_test@test.com";
    public const string DefaultUserEmail = "test_admin@test.com";
    public const string SourceSystem = "BddTest";

    // FeatureContants handle later in service Tenancy

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
    private bool _disposed = false;
    private readonly TestServer _server;
    private readonly IServiceScope _serviceScope;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly Dictionary<string, UserData> _users = new Dictionary<string, UserData>();
    private readonly TestUserPermissionProvider _testUserPermissionProvider;

    private UserData _defaultUser
        = new UserData("TEST_ADMIN", "TEST_ADMIN", DefaultTenantId, isSupportUser: true);

    protected ApiTestContextBase() : this(startupFactory: null)
    {
    }

    protected ApiTestContextBase(Func<WebHostBuilderContext, TStartup> startupFactory)
    {
        MutableMemoryConfigurationProvider mutableMemoryConfigurationProvider = new(new Dictionary<string, string>());

        var webHostBuilder = new WebHostBuilder()
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<HttpClient>(sp => this.GetClientWithHeaderPropagation());
                services.AddSingleton(mutableMemoryConfigurationProvider);
                services.AddSerilog(SetupLogging);
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.Add(mutableMemoryConfigurationProvider);
                config.AddUserSecrets<TStartup>();
            });

        webHostBuilder = startupFactory is not null
            ? webHostBuilder.UseStartup(startupFactory)
            : webHostBuilder.UseStartup<TStartup>();

        webHostBuilder = webHostBuilder.ConfigureLogging(builder => builder.ClearProviders().AddSerilog());

        _server = new TestServer(webHostBuilder);

        _serviceScope = _server.Host.Services.CreateScope();
        _logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ApiTestContextBase<TStartup>>();

        _testUserPermissionProvider = ServiceProvider.GetRequiredService<IUserPermissionsProvider>() as TestUserPermissionProvider;
    }

    public Microsoft.Extensions.Logging.ILogger Logger => _logger;

    public ILoggerFactory LoggerFactory => ServiceProvider.GetRequiredService<ILoggerFactory>();

    public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

    public Exception LastApiError { get; set; }

    public HttpClient Client => GetAuthorizedClient(_defaultUser);

    public string LoggedInUser { get; set; }

    public HttpStatusCode LastStatusCode { get; set; }


    #region Manage User Management
    public void AddUser(UserData user, bool setAsDefault)
    {
        _users.Add(user.Username, user);
        if (setAsDefault)
        {
            _defaultUser = user;
            _logger.LogWarning("Changing default user to '{username}'({tenantId})", user.Username, user.TenantId);
        }
    }

    public void AddOrUpdateUser(UserData user, bool setAsDefault)
    {
        if (_users.ContainsKey(user.Username))
        {
            _users[user.Username] = user;
        }
        else
        {
            _users.Add(user.Username, user);
        }
        if (setAsDefault)
        {
            _defaultUser = user;
            _logger.LogWarning("Changing default user to '{username}'({tenantId})", user.Username, user.TenantId);
        }
    }

    public UserData GetUserByUsername(string? username = null)
    {
        var operationalUsername = username ?? LoggedInUser;
        operationalUsername = operationalUsername?.ToLower();
        if (string.IsNullOrEmpty(operationalUsername) || string.Equals(username, _defaultUser.Username, StringComparison.OrdinalIgnoreCase))
        {
            return _defaultUser;
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

    public void AddPermissionToUser(string userId, string permissionId)
    {
        _testUserPermissionProvider.AddPermission(userId, permissionId);
    }

    public void SetupPermissionsForUser(string username, string[] permissionIds)
    {
        var user = GetUserByUsername(username);
        foreach (var permissionId in permissionIds)
        {
            AddPermissionToUser(user.Id, permissionId);
        }
    }

    public void GrantAllPermissionsToUser(string userId)
    {
        foreach (var permissionId in AllPermissionIds)
        {
            _testUserPermissionProvider.AddPermission(userId, permissionId);
        }
    }
    #endregion

    #region HTTP Client Management
    private HttpClient GetClientWithHeaderPropagation()
    {
        var client = _server.CreateClient();
        client.DefaultRequestHeaders.Accept.Clear();

        // Simulate Header Propagation middleware which doesn't work on the test client
        var httpContext = _server.Services.GetService<IHttpContextAccessor>()?.HttpContext;
        if (httpContext != null
            && httpContext.Request.Headers.TryGetValue("Authorization", out var values)
            && !StringValues.IsNullOrEmpty(values))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", (string)values);
        }
        return client;
    }

    public HttpClient GetAuthorizedClient(UserData? user, string acceptHeader = "application/json")
    {
        return GetAuthorisedClientCore(user, acceptHeader);
    }

    private HttpClient GetAuthorisedClientCore(UserData? user, string acceptHeader)
    {
        var client = _server.CreateClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.ParseAdd(acceptHeader);

        if (user?.UserType is UserTypes.AppClientWithIndividualUsers or UserTypes.AppClientWithoutIndividualUsers)
        {
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.UserTypeCustomHeaderName, user.UserType);
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.TenantIdCustomHeaderName, user.TenantId);
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.UserIdCustomHeaderName, user.Id);
            client.DefaultRequestHeaders.Add(HttpRequestUserDataProvider.ActionUserIdCustomHeaderName, user.Username);
        }

        user ??= _defaultUser;
        return SystemInternalJwtTokenFactory.AddUserContext(client, user);
    }
    #endregion

    #region Http Action Method
    public async Task<Result<TResponse>> GetAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var client = GetAuthorizedClient(user);
            var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            using var response = await client.GetAsync(relativeUri);

            var result = await ProcessResultResponse<TResponse>(response);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during GET request to {relativeUri}", relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    public async Task<Result> PostAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
        where TRequest : ICommand
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var client = GetAuthorizedClient(user);
            var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _logger.LogInformation(
                    "Sending REQUEST as '{username}': POST {relativeUri} {jsonBody}",
                    user?.Username ?? _defaultUser?.Username,
                    relativeUri);

            using var response = await client.PostAsync(relativeUri, requestBody);

            return await ProcessResultResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during POST request to {relativeUri}", relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
        where TRequest : ICommand<TResponse>
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var client = GetAuthorizedClient(user);
            var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _logger.LogInformation(
                    "Sending REQUEST as '{username}': POST {relativeUri} {jsonBody}",
                    user?.Username ?? _defaultUser?.Username,
                    relativeUri);

            using var response = await client.PostAsync(relativeUri, requestBody);

            return await ProcessResultResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during POST request to {relativeUri}", relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    public async Task<Result> PutAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
        where TRequest : ICommand
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var client = GetAuthorizedClient(user);
            var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _logger.LogInformation(
                    "Sending REQUEST as '{username}': PUT {relativeUri} {jsonBody}",
                    user?.Username ?? _defaultUser?.Username,
                    relativeUri);

            using var response = await client.PutAsync(relativeUri, requestBody);

            return await ProcessResultResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during PUT request to {relativeUri}", relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    public async Task<Result<TResponse>> PutAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
        where TRequest : ICommand<TResponse>
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var client = GetAuthorizedClient(user);
            var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _logger.LogInformation(
                    "Sending REQUEST as '{username}': PUT {relativeUri} {jsonBody}",
                    user?.Username ?? _defaultUser?.Username,
                    relativeUri);

            using var response = await client.PutAsync(relativeUri, requestBody);

            return await ProcessResultResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during PUT request to {relativeUri}", relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    public async Task<Result> PatchAsync<TRequest>(string relativeUri, TRequest request, UserData? user = null)
        where TRequest : ICommand
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var client = GetAuthorizedClient(user);
            var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _logger.LogInformation(
                    "Sending REQUEST as '{username}': PATCH {relativeUri} {jsonBody}",
                    user?.Username ?? _defaultUser?.Username,
                    relativeUri);

            using var response = await client.PatchAsync(relativeUri, requestBody);

            return await ProcessResultResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during PATCH request to {relativeUri}", relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    public async Task<Result<TResponse>> PatchAsync<TRequest, TResponse>(string relativeUri, TRequest request, UserData? user = null)
        where TRequest : ICommand<TResponse>
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var client = GetAuthorizedClient(user);
            var requestBody = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            _logger.LogInformation(
                    "Sending REQUEST as '{username}': PATCH {relativeUri} {jsonBody}",
                    user?.Username ?? _defaultUser?.Username,
                    relativeUri);

            using var response = await client.PatchAsync(relativeUri, requestBody);

            return await ProcessResultResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during PATCH request to {relativeUri}", relativeUri);
            this.LastApiError = ex;
            throw;
        }
    }

    public async Task<Result> DeleteAsync<TRequest>(string relativeUri, UserData? user = null)
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUri))
                throw new ArgumentNullException(nameof(relativeUri));

            var client = GetAuthorizedClient(user);

            _logger.LogInformation(
                    "Sending REQUEST as '{username}': PUT {relativeUri} {jsonBody}",
                    user?.Username ?? _defaultUser?.Username,
                    relativeUri);

            using var response = await client.DeleteAsync(relativeUri);

            return await ProcessResultResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during PUT request to {relativeUri}", relativeUri);
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

    #endregion

    #region Logging
    private static void SetupLogging(IServiceProvider serviceProvider, LoggerConfiguration loggerConfig)
    {
        loggerConfig
            .MinimumLevel.Debug()
            .Filter.ByExcluding(e => e.MessageTemplate.Text.Contains("does not implement 'IIdentifiable'", StringComparison.InvariantCulture))
            .Filter.ByExcluding(e => e.MessageTemplate.Text.StartsWith("Executing endpoint", StringComparison.InvariantCulture))
            .Filter.ByExcluding(e => e.MessageTemplate.Text.StartsWith("Route matched with", StringComparison.InvariantCulture))
            .Filter.ByExcluding(e => e.MessageTemplate.Text.StartsWith("Executed action", StringComparison.InvariantCulture))
            .Filter.ByExcluding(e => e.MessageTemplate.Text.StartsWith("Executed endpoint", StringComparison.InvariantCulture))
            .Filter.ByExcluding(e => e.MessageTemplate.Text.StartsWith("Executing ObjectResult", StringComparison.InvariantCulture))
            .Filter.ByExcluding(e => e.MessageTemplate.Text.StartsWith("Configured endpoint test_queue", StringComparison.InvariantCulture))
            .Filter.ByExcluding(e => CheckLogContextProperty(e.Properties, "RequestPath", "\"/api/v1/userPermissions\""))
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .ReadFrom.Configuration(serviceProvider.GetRequiredService<IConfiguration>())
            .WriteTo.Debug(levelSwitch: new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Warning))
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] (T={ThreadId}) {Message:lj} {Properties}{NewLine}{Exception}{NewLine}");
    }

    private static bool CheckLogContextProperty(
        IReadOnlyDictionary<string, Serilog.Events.LogEventPropertyValue> logContextProperties,
        string propertyName,
        string targetTextValue)
    {
        bool doesMatchTargetValue = false;
        if (logContextProperties.ContainsKey(propertyName))
        {
            doesMatchTargetValue = logContextProperties[propertyName].ToString().Equals(targetTextValue, StringComparison.InvariantCultureIgnoreCase);
        }
        return doesMatchTargetValue;
    }

    #endregion

    #region Dispose
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _serviceScope.Dispose();

            // the factory tracks and disposes of any clients created
            _server.Dispose();
        }

        _disposed = true;
    }

    #endregion
}