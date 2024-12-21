using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EShop.Testing.JsonApiApplication;

public interface ITestDatabaseConnectionInterceptor : IDbConnectionInterceptor
{
}

public sealed class TestDatabaseConnectionInterceptor : ITestDatabaseConnectionInterceptor
{
}