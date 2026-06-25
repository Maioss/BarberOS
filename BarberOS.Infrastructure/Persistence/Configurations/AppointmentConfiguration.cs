using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<AppointmentDbModel>
    {
        public void Configure(EntityTypeBuilder<AppointmentDbModel> builder)
        {
            builder.ToTable("appointments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClientId).IsRequired();
            builder.Property(x => x.BarberId).IsRequired();
            builder.Property(x => x.BarbershopId).IsRequired();
            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.StartTime).IsRequired();
            builder.Property(x => x.EndTime).IsRequired();
            builder.Property(x => x.TotalPrice).IsRequired().HasColumnType("numeric(12,2)");
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();

            builder.HasIndex(x => new { x.BarberId, x.Date });
            builder.HasIndex(x => x.ClientId);
            builder.HasIndex(x => x.BarbershopId);

            builder.HasMany(x => x.Services)
                .WithOne(x => x.Appointment)
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<BarberDbModel>()
                .WithMany()
                .HasForeignKey(x => x.BarberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<BarbershopDbModel>()
                .WithMany()
                .HasForeignKey(x => x.BarbershopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<UserDbModel>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
