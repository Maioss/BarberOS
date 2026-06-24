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

            if (!await db.Users.AnyAsync(u => u.Role == (int)Role.SuperAdmin, ct))
            {
                var superAdmin = User.Create(
                    email: "superadmin@barberos.com",
                    passwordHash: hasher.Hash("Admin123!"),
                    fullName: "Super Admin",
                    role: Role.SuperAdmin);

                await db.Users.AddAsync(UserMapper.ToDbModel(superAdmin), ct);
                await db.SaveChangesAsync(ct);
            }

            if (!await db.Barbershops.AnyAsync(ct))
            {
                var medellin = Barbershop.CreateMain("BarberOS Medellín Centro", "Calle 52 #43-15", "Medellín", "+574123456");
                var bogota = Barbershop.CreateMain("BarberOS Bogotá Chapinero", "Cra 13 #63-52", "Bogotá", "+571987654");

                await db.Barbershops.AddAsync(BarbershopMapper.ToDbModel(medellin), ct);
                await db.Barbershops.AddAsync(BarbershopMapper.ToDbModel(bogota), ct);
                await db.SaveChangesAsync(ct);

                var medellinSur = Barbershop.CreateBranch("BarberOS Medellín Sur", "Av El Poblado #1-50", "Medellín", "+574111222", medellin.Id);
                var medellinNorte = Barbershop.CreateBranch("BarberOS Medellín Norte", "Calle 77 #50-20", "Medellín", "+574333444", medellin.Id);
                var bogotaUsaquen = Barbershop.CreateBranch("BarberOS Bogotá Usaquén", "Cra 6 #119-40", "Bogotá", "+571555666", bogota.Id);
                var bogotaKennedy = Barbershop.CreateBranch("BarberOS Bogotá Kennedy", "Av Primera de Mayo #65-20", "Bogotá", "+571777888", bogota.Id);

                await db.Barbershops.AddAsync(BarbershopMapper.ToDbModel(medellinSur), ct);
                await db.Barbershops.AddAsync(BarbershopMapper.ToDbModel(medellinNorte), ct);
                await db.Barbershops.AddAsync(BarbershopMapper.ToDbModel(bogotaUsaquen), ct);
                await db.Barbershops.AddAsync(BarbershopMapper.ToDbModel(bogotaKennedy), ct);
                await db.SaveChangesAsync(ct);
            }

            if (!await db.Users.AnyAsync(u => u.Role == (int)Role.Admin, ct))
            {
                var firstMain = await db.Barbershops.FirstAsync(b => b.IsMain, ct);

                var admin = User.Create(
                    email: "admin@barberos.com",
                    passwordHash: hasher.Hash("Admin123!"),
                    fullName: "Admin Demo",
                    role: Role.Admin,
                    phone: null,
                    barbershopId: firstMain.Id);

                var client1 = User.Create("cliente1@demo.com", hasher.Hash("Cliente123!"), "Carlos Cliente", Role.Client);
                var client2 = User.Create("cliente2@demo.com", hasher.Hash("Cliente123!"), "Camila Cliente", Role.Client);

                await db.Users.AddAsync(UserMapper.ToDbModel(admin), ct);
                await db.Users.AddAsync(UserMapper.ToDbModel(client1), ct);
                await db.Users.AddAsync(UserMapper.ToDbModel(client2), ct);
                await db.SaveChangesAsync(ct);
            }

            if (!await db.Barbers.AnyAsync(ct))
            {
                var branches = await db.Barbershops
                    .Where(b => !b.IsMain && b.IsActive)
                    .OrderBy(b => b.CreatedAt)
                    .ToListAsync(ct);

                var barberUser1 = User.Create(
                    email: "barber1@barberos.com",
                    passwordHash: hasher.Hash("Barber123!"),
                    fullName: "Andres Barber",
                    role: Role.Barber,
                    phone: "+573001234567",
                    barbershopId: branches[0].Id);

                var barberUser2 = User.Create(
                    email: "barber2@barberos.com",
                    passwordHash: hasher.Hash("Barber123!"),
                    fullName: "Luis Barber",
                    role: Role.Barber,
                    phone: "+573007654321",
                    barbershopId: branches[1].Id);

                var barberUser3 = User.Create(
                    email: "barber3@barberos.com",
                    passwordHash: hasher.Hash("Barber123!"),
                    fullName: "Maria Barber",
                    role: Role.Barber,
                    phone: "+573009876543",
                    barbershopId: branches[2].Id);

                await db.Users.AddAsync(UserMapper.ToDbModel(barberUser1), ct);
                await db.Users.AddAsync(UserMapper.ToDbModel(barberUser2), ct);
                await db.Users.AddAsync(UserMapper.ToDbModel(barberUser3), ct);
                await db.SaveChangesAsync(ct);

                var barber1 = Barber.Create(barberUser1.Id, branches[0].Id);
                var barber2 = Barber.Create(barberUser2.Id, branches[1].Id);
                var barber3 = Barber.Create(barberUser3.Id, branches[2].Id);

                await db.Barbers.AddAsync(BarberMapper.ToDbModel(barber1), ct);
                await db.Barbers.AddAsync(BarberMapper.ToDbModel(barber2), ct);
                await db.Barbers.AddAsync(BarberMapper.ToDbModel(barber3), ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
