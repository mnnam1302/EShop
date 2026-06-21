using EShop.Shared.Contracts.Abstractions.MessageBus;
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

    public static IServiceCollection AddCommandBus(this IServiceCollection services)
    {
        services.AddScoped<ICommandBus, CommandBus>();
        return services;
    }

    public static ModelBuilder AddInboxMessageEntity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InboxMessageEntityTypeConfiguration());
        return modelBuilder;
    }

    public static ModelBuilder AddOutboxMessageEntity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        return modelBuilder;
    }
}
