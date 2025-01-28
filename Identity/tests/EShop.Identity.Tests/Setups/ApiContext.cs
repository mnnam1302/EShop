using EShop.Testing.JsonApiApplication;

namespace EShop.Identity.Tests.Setups;

// Research more: https://xunit.net/docs/running-tests-in-parallel#runners-and-test-frameworks
public sealed class ApiContext : ApiTestContextBase<TestStartup>
{
    public ApiContext(PostgreSqlTestDatabase testDatabase)
        : base(startupFactory: context => 
            new TestStartup(context.Configuration, context.HostingEnvironment, testDatabase))
    {
    }
}