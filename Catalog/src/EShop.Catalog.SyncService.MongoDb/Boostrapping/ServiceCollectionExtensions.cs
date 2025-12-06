using EShop.Catalog.SyncService.MongoDb.Abstractions;
using EShop.Catalog.SyncService.MongoDb.Infrastructure;
using EShop.Catalog.SyncService.MongoDb.Infrastructure.Repository;
using Microsoft.Extensions.Options;

namespace EShop.Catalog.SyncService.MongoDb.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoPersistence(this IServiceCollection services)
    {
        services.AddOptions<MongoDbSettings>();
        
        services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        return services;
    }
}
