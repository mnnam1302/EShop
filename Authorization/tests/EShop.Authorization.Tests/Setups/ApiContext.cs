using EShop.Testing.JsonApiApplication;

namespace EShop.Authorization.Tests.Setups;

public sealed class ApiContext : ApiTestContextBase<TestStartup>
{
    public ApiContext(PostgreSqlTestDatabase testDatabase)
       : base(startupFactory: context => new TestStartup(context.Configuration, context.HostingEnvironment, testDatabase))
    {
    }
}
