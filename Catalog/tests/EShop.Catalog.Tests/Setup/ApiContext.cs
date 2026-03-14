using EShop.Testing.JsonApiApplication;

namespace EShop.Catalog.Tests.Setup;

public sealed class ApiContext : ApiTestContextBase<TestStartup>
{
    public ApiContext(PostgreSqlTestDatabase testDatabase, MongoDbTestDatabase mongoDatabase)
       : base(startupFactory: context => new TestStartup(context.Configuration, context.HostingEnvironment, testDatabase, mongoDatabase))
    {
    }
}