using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EShop.Shared.ReadModel.EfCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the EF Core-backed read model projection infrastructure:
    /// <see cref="IReadModelStore{TReadModel}"/>, <see cref="IReadModelProjector{TReadModel}"/>,
    /// and a <see cref="PropertyReadModelLocator{TReadModel}"/> using the specified property name.
    /// </summary>
    /// <typeparam name="TReadModel">The read model type.</typeparam>
    /// <typeparam name="TDbContext">The EF Core DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="readModelIdPropertyName">The event property name containing the read model ID (e.g. "ProductId").</param>
    public static IServiceCollection UseEfCoreReadModelStore<TReadModel, TDbContext>(
        this IServiceCollection services,
        string readModelIdPropertyName)
        where TReadModel : class, IReadModel, new()
        where TDbContext : DbContext
    {
        services.TryAddSingleton<IReadModelLocator<TReadModel>>(
            new PropertyReadModelLocator<TReadModel>(readModelIdPropertyName));

        services.TryAddScoped<IReadModelStore<TReadModel>, EfCoreReadModelStore<TReadModel, TDbContext>>();
        services.TryAddScoped<IReadModelProjector<TReadModel>, ReadModelProjector<TReadModel>>();

        return services;
    }
}