using EShop.Shared.Authentication.Abstractions;
using EShop.Testing.JsonApiApplication;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Catalog.Tests.Setup;

public sealed class ApiContext : ApiTestContextBase<TestStartup>
{
    public ApiContext(PostgreSqlTestDatabase testDatabase, MongoDbTestDatabase mongoDatabase)
       : base(startupFactory: context => new TestStartup(context.Configuration, context.HostingEnvironment, testDatabase, mongoDatabase))
    {
    }

    /// <summary>
    /// Executes a read model query within a system user scope so that the
    /// <see cref="IUserDetailsProvider"/> is authenticated and tenant-scoped.
    /// Use this in BDD assertion steps that query repositories directly
    /// (outside of an HTTP request pipeline).
    /// </summary>
    public async Task<T> QueryReadModelAsync<T>(Func<IServiceProvider, Task<T>> query)
    {
        var userDetailsProvider = ServiceProvider.GetRequiredService<IUserDetailsProvider>();

        using var _ = userDetailsProvider.CreateSystemUserScope(DefaultTenantId);

        return await query(ServiceProvider);
    }
}