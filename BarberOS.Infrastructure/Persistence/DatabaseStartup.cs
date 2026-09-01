using BarberOS.Application.Shared;
using BarberOS.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BarberOS.Infrastructure.Persistence
{
    public class DatabaseStartupOptions
    {
        public bool ApplyMigrations { get; set; }
        public bool SeedDemoData { get; set; }
    }

    public static class DatabaseStartup
    {
        public static DatabaseStartupOptions ReadStartupOptions(this IConfiguration configuration, bool isDevelopment)
        {
            var section = configuration.GetSection("Database");

            return new DatabaseStartupOptions
            {
                ApplyMigrations = section.GetValue("ApplyMigrationsOnStartup", isDevelopment),
                SeedDemoData = section.GetValue("SeedDemoData", isDevelopment)
            };
        }

        public static async Task PrepareDatabaseAsync(
            this IServiceProvider services,
            DatabaseStartupOptions options,
            CancellationToken ct = default)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BarberOSDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BarberOSDbContext>>();

            if (options.ApplyMigrations)
            {
                await db.Database.MigrateAsync(ct);
            }
            else
            {
                var pending = await db.Database.GetPendingMigrationsAsync(ct);
                if (pending.Any())
                    throw new InvalidOperationException(
                        $"La base tiene {pending.Count()} migraciones sin aplicar y ApplyMigrationsOnStartup esta desactivado. " +
                        "Ejecuta 'dotnet ef database update' antes de arrancar.");
            }

            if (!options.SeedDemoData)
            {
                logger.LogInformation("Siembra de datos de demo desactivada.");
                return;
            }

            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DataSeeder.SeedAsync(db, hasher, ct);
        }
    }
}
