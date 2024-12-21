using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Testing.JsonApiApplication.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlDbContext<TContext>(
        this IServiceCollection services,
        PostgreSqlTestDatabase testDatabase,
        Action<IServiceProvider, DbContextOptionsBuilder> addtionalDbContextConfig = null)
    {
        services.AddSingleton(testDatabase);
    }
}