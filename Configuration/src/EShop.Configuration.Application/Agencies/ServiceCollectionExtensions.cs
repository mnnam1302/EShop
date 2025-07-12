namespace EShop.Configuration.Application.Agencies;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgencies(this IServiceCollection services)
    {
        services.AddScoped<IAgencyRepository, AgencyRepository>();
        return services;
    }
}
