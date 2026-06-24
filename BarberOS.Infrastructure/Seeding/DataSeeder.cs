using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Infrastructure.Persistence;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Seeding
{

    public static class DataSeeder
    {
        public static async Task SeedAsync(BarberOSDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
        {
            await db.Database.MigrateAsync(ct);

            if (await db.Users.AnyAsync(ct)) return;

            var superAdmin = User.Create(
                email: "superadmin@barberos.com",
                passwordHash: hasher.Hash("Admin123!"),
                fullName: "Super Admin",
                role: Role.SuperAdmin);

            await db.Users.AddAsync(UserMapper.ToDbModel(superAdmin), ct);
            await db.SaveChangesAsync(ct);
        }
    }
}
