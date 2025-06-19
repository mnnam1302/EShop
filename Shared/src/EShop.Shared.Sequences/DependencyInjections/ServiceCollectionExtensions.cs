using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EShop.Shared.Sequences.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSequenceManagement<TSequenceStore>(this IServiceCollection services)
        where TSequenceStore : class, ISequenceStore
    {
        static void DefaultOptions(SequenceManagerOptions options)
        {
            // No default required
        }

        return AddSequenceManagement<TSequenceStore>(services, DefaultOptions);
    }

    public static IServiceCollection AddSequenceManagement<TSequenceStore>(this IServiceCollection services, Action<SequenceManagerOptions> options)
        where TSequenceStore : class, ISequenceStore
    {
        services.Configure<SequenceManagerOptions>(options);

        services.AddTransient<ISequenceManager, SequenceManager>();
        services.AddScoped<ISequenceStore, TSequenceStore>();
        services.AddTransient(provider => new Func<string, SequenceRange>((sequenceKey) =>
            new SequenceRange(
                sequenceKey,
                provider.GetRequiredService<IOptions<SequenceManagerOptions>>())
        ));

        services.AddSingleton<SequenceRangeInMemoryCache>();
        services.AddTransient<ISequenceRegistry, GenericSequenceRegistry>();
        services.AddTransient<IUniqueReferenceGenerator, GenericUniqueReferenceGenerator>();

        return services;
    }
}