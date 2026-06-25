using BarberOS.Application.Shared;
using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence
{

    public class BarberOSDbContext : DbContext, IUnitOfWork
    {
        public BarberOSDbContext(DbContextOptions<BarberOSDbContext> options) : base(options) { }

        public DbSet<UserDbModel> Users => Set<UserDbModel>();
        public DbSet<BarbershopDbModel> Barbershops => Set<BarbershopDbModel>();
        public DbSet<BarberDbModel> Barbers => Set<BarberDbModel>();
        public DbSet<ServiceDbModel> Services => Set<ServiceDbModel>();
        public DbSet<AppointmentDbModel> Appointments => Set<AppointmentDbModel>();
        public DbSet<AppointmentServiceDbModel> AppointmentServices => Set<AppointmentServiceDbModel>();
        public DbSet<PaymentDbModel> Payments => Set<PaymentDbModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarberOSDbContext).Assembly);
        }
    }
}
