using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<PaymentDbModel>
    {
        public void Configure(EntityTypeBuilder<PaymentDbModel> builder)
        {
            builder.ToTable("payments");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AppointmentId).IsRequired();
            builder.Property(x => x.ClientId).IsRequired();
            builder.Property(x => x.BarberId).IsRequired();
            builder.Property(x => x.BarbershopId).IsRequired();
            builder.Property(x => x.Amount).IsRequired().HasColumnType("numeric(12,2)");
            builder.Property(x => x.Method).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();

            builder.HasIndex(x => x.AppointmentId);
            builder.HasIndex(x => x.ClientId);
            builder.HasIndex(x => x.BarberId);
            builder.HasIndex(x => x.BarbershopId);

            builder.HasOne(x => x.Appointment)
                .WithMany()
                .HasForeignKey(x => x.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

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
