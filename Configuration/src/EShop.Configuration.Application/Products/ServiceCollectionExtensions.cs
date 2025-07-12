namespace EShop.Configuration.Application.Products;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProducts(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }
}
