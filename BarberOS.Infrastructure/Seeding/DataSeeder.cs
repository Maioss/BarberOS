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
            if (!await db.Users.AnyAsync(u => u.Role == (int)Role.SuperAdmin, ct))
            {
                var superAdmin = User.Create(
                    email: "samin@barberos.com",
                    passwordHash: hasher.Hash("Pitch2026!"),
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
                var firstMain = await db.Barbershops
                    .Where(b => b.IsMain)
                    .OrderBy(b => b.CreatedAt)
                    .FirstAsync(ct);

                var admin = User.Create(
                    email: "admin.pitch@barberos.com",
                    passwordHash: hasher.Hash("Pitch2026!"),
                    fullName: "Admin Demo",
                    role: Role.Admin,
                    phone: null,
                    barbershopId: firstMain.Id);

                var client1 = User.Create("cliente.pitch@barberos.com", hasher.Hash("Pitch2026!"), "Carlos Cliente", Role.Client);
                var client2 = User.Create("cliente2@barberos.com", hasher.Hash("Pitch2026!"), "Camila Cliente", Role.Client);

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
                if (branches.Count < 3)
                    return;

                var barberUser1 = User.Create(
                    email: "barbero.pitch@barberos.com",
                    passwordHash: hasher.Hash("Pitch2026!"),
                    fullName: "Andrés Barbero",
                    role: Role.Barber,
                    phone: "+573001234567",
                    barbershopId: branches[0].Id);

                var barberUser2 = User.Create(
                    email: "barbero2@barberos.com",
                    passwordHash: hasher.Hash("Pitch2026!"),
                    fullName: "Luis Barbero",
                    role: Role.Barber,
                    phone: "+573007654321",
                    barbershopId: branches[1].Id);

                var barberUser3 = User.Create(
                    email: "barbero3@barberos.com",
                    passwordHash: hasher.Hash("Pitch2026!"),
                    fullName: "María Barbero",
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

            if (!await db.Services.AnyAsync(ct))
            {
                var principales = await db.Barbershops.Where(b => b.IsMain).ToListAsync(ct);

                foreach (var shop in principales)
                {
                    var corte = Service.Create(shop.Id, "Corte de cabello", "Corte clásico o moderno", 25000m, 30);
                    var barba = Service.Create(shop.Id, "Arreglo de barba", "Perfilado y diseño", 15000m, 20);
                    var cejas = Service.Create(shop.Id, "Cejas", "Depilación y diseño", 8000m, 10);
                    var combo = Service.Create(shop.Id, "Corte + Barba", "Combo completo", 35000m, 45);

                    foreach (var svc in new[] { corte, barba, cejas, combo })
                        await db.Services.AddAsync(ServiceMapper.ToDbModel(svc), ct);
                }

                await db.SaveChangesAsync(ct);
            }

            // Appointments: seeded separately — works even if users were created in a prior run
            if (!await db.Appointments.AnyAsync(ct))
            {
                var today = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById(Barbershop.DefaultTimeZoneId)));

                var client = await db.Users.FirstOrDefaultAsync(u => u.Role == (int)Role.Client && u.IsActive, ct);
                var barbers = await db.Barbers.Where(b => b.IsActive).Take(2).ToListAsync(ct);
                var serviceDbs = await db.Services.Where(s => s.IsActive).Take(4).ToListAsync(ct);

                if (client == null || barbers.Count == 0 || serviceDbs.Count == 0)
                    return;

                var b1 = barbers[0];
                var b2 = barbers.Count > 1 ? barbers[1] : barbers[0];

                var s1 = ServiceMapper.ToDomain(serviceDbs[0]);
                var s2 = ServiceMapper.ToDomain(serviceDbs.Count > 1 ? serviceDbs[1] : serviceDbs[0]);
                var s3 = ServiceMapper.ToDomain(serviceDbs.Count > 2 ? serviceDbs[2] : serviceDbs[0]);
                var s4 = ServiceMapper.ToDomain(serviceDbs.Count > 3 ? serviceDbs[3] : serviceDbs[0]);

                // ── Citas confirmadas ──────────────────────────────────────
                var day1 = NextWorkingDay(BarberMapper.ToDomain(b1), today);
                var day2 = NextWorkingDay(BarberMapper.ToDomain(b2), today);

                var apt1 = Appointment.Create(client.Id, b1.Id, b1.BarbershopId, day1, new TimeOnly(9, 0), new List<Service> { s1 }.AsReadOnly());
                var apt2 = Appointment.Create(client.Id, b1.Id, b1.BarbershopId, day1, new TimeOnly(10, 0), new List<Service> { s2 }.AsReadOnly());
                var apt3 = Appointment.Create(client.Id, b2.Id, b2.BarbershopId, day2, new TimeOnly(11, 0), new List<Service> { s1, s2 }.AsReadOnly());

                await db.Appointments.AddAsync(AppointmentMapper.ToDbModel(apt1), ct);
                await db.Appointments.AddAsync(AppointmentMapper.ToDbModel(apt2), ct);
                await db.Appointments.AddAsync(AppointmentMapper.ToDbModel(apt3), ct);

                // ── Históricas: completadas ─────────────────────────────────
                var historical = new (int daysAgo, TimeOnly time, Service svc)[]
                {
                    (-4,  new TimeOnly(9,  0),  s1),
                    (-7,  new TimeOnly(10, 0),  s2),
                    (-11, new TimeOnly(14, 0),  s3),
                    (-15, new TimeOnly(9,  30), s4),
                    (-20, new TimeOnly(11, 0),  s1),
                    (-26, new TimeOnly(15, 0),  s2),
                    (-33, new TimeOnly(9,  0),  s3),
                    (-40, new TimeOnly(10, 30), s4),
                    (-47, new TimeOnly(14, 0),  s1),
                    (-55, new TimeOnly(9,  0),  s2),
                };

                foreach (var (daysAgo, time, svc) in historical)
                {
                    var hist = Appointment.Create(client.Id, b1.Id, b1.BarbershopId, today.AddDays(daysAgo), time, new List<Service> { svc }.AsReadOnly());
                    hist.Complete(today);
                    await db.Appointments.AddAsync(AppointmentMapper.ToDbModel(hist), ct);
                    var credit = BalanceEntry.ForCompletedAppointment(b1.Id, hist.Id, hist.TotalPrice);
                    await db.BalanceEntries.AddAsync(BalanceEntryMapper.ToDbModel(credit), ct);
                }

                // ── Canceladas: para variedad en métricas ──────────────────
                var c1 = Appointment.Create(client.Id, b2.Id, b2.BarbershopId, today.AddDays(-3), new TimeOnly(9, 0), new List<Service> { s1 }.AsReadOnly());
                c1.Cancel();
                var c2 = Appointment.Create(client.Id, b1.Id, b1.BarbershopId, today.AddDays(-9), new TimeOnly(16, 0), new List<Service> { s2 }.AsReadOnly());
                c2.Cancel();

                await db.Appointments.AddAsync(AppointmentMapper.ToDbModel(c1), ct);
                await db.Appointments.AddAsync(AppointmentMapper.ToDbModel(c2), ct);

                await db.SaveChangesAsync(ct);
            }
        }
        private static DateOnly NextWorkingDay(Barber barber, DateOnly from)
        {
            for (var i = 0; i < 7; i++)
            {
                var candidate = from.AddDays(i);
                if (barber.IsAvailableOn(candidate.DayOfWeek))
                    return candidate;
            }
            return from;
        }
    }
}
