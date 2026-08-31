using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;
using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BarberOS.Infrastructure.Persistence
{

    public class BarberOSDbContext : DbContext, IUnitOfWork
    {
        private const string ExclusionViolation = "23P01";
        private const string UniqueViolation = "23505";

        public BarberOSDbContext(DbContextOptions<BarberOSDbContext> options) : base(options) { }

        public DbSet<UserDbModel> Users => Set<UserDbModel>();
        public DbSet<BarbershopDbModel> Barbershops => Set<BarbershopDbModel>();
        public DbSet<BarberDbModel> Barbers => Set<BarberDbModel>();
        public DbSet<ServiceDbModel> Services => Set<ServiceDbModel>();
        public DbSet<AppointmentDbModel> Appointments => Set<AppointmentDbModel>();
        public DbSet<AppointmentServiceDbModel> AppointmentServices => Set<AppointmentServiceDbModel>();
        public DbSet<PaymentDbModel> Payments => Set<PaymentDbModel>();
        public DbSet<BalanceEntryDbModel> BalanceEntries => Set<BalanceEntryDbModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarberOSDbContext).Assembly);
        }

        /// <summary>
        /// Traduce las restricciones que impone Postgres a excepciones de dominio, para
        /// que Application no sepa de Npgsql y el cliente reciba un 409 en vez de un 500.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                return await base.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
            {
                throw Translate(pg, ex);
            }
        }

        private static Exception Translate(PostgresException pg, Exception original) => pg.SqlState switch
        {
            ExclusionViolation when pg.ConstraintName == "appointments_no_overlap" =>
                new ConflictException("El barbero ya tiene una reserva confirmada que se cruza con ese horario."),

            ExclusionViolation =>
                new ConflictException("La operación choca con un registro existente."),

            UniqueViolation when pg.ConstraintName is not null
                && pg.ConstraintName.StartsWith("IX_barber_balance_entries") =>
                new ConflictException("Ese movimiento de saldo ya estaba registrado."),

            _ => original
        };
    }
}
