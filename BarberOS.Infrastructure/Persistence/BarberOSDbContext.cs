using BarberOS.Application.Shared;
using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence
{

    public class BarberOSDbContext : DbContext, IUnitOfWork
    {
        public BarberOSDbContext(DbContextOptions<BarberOSDbContext> options) : base(options) { }

        public DbSet<UserDbModel> Users => Set<UserDbModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarberOSDbContext).Assembly);
        }
    }
}
