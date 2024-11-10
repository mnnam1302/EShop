using EShop.Identity.Persistence;
using EShop.Identity.Persistence.SeedingData;
using Microsoft.EntityFrameworkCore;

namespace EShop.Identity.API.DependencyInjections.Extensions;

public static class MigrationExtensions
{
    public static async void ApplyMigrations(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            using UserDbContext dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();

            await dbContext.Database.MigrateAsync();
            await dbInitializer.Initialize();
        }
    }
}