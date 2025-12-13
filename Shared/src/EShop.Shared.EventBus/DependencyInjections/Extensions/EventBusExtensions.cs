using EShop.Shared.DomainTools.EventSourcing.Configurations;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.EventBus.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.EventBus.DependencyInjections.Extensions;

public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        services.AddScoped<IEventBus, EventBus>();
        return services;
    }

    public static ModelBuilder AddInboxMessageEntity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageEntityTypeConfiguration());
        return modelBuilder;
    }

    public static IServiceCollection AddPostgreSqlIdempotentConsumer<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext, IInboxDbContext
    {
        services.AddScoped<IMessageRepository, PostgreSqlMessageRepository<TDbContext>>();
        return services;
    }
}