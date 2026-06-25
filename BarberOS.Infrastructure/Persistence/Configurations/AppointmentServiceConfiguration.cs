using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{
    public class AppointmentServiceConfiguration : IEntityTypeConfiguration<AppointmentServiceDbModel>
    {
        public void Configure(EntityTypeBuilder<AppointmentServiceDbModel> builder)
        {
            builder.ToTable("appointment_services");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AppointmentId).IsRequired();
            builder.Property(x => x.ServiceId).IsRequired();
            builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(80);
            builder.Property(x => x.Price).IsRequired().HasColumnType("numeric(12,2)");
            builder.Property(x => x.DurationMinutes).IsRequired();
        }
    }
}
