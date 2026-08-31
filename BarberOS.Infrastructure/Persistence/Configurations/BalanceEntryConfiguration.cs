using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{
    public class BalanceEntryConfiguration : IEntityTypeConfiguration<BalanceEntryDbModel>
    {
        public void Configure(EntityTypeBuilder<BalanceEntryDbModel> builder)
        {
            builder.ToTable("barber_balance_entries");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BarberId).IsRequired();
            builder.Property(x => x.Amount).IsRequired().HasColumnType("numeric(12,2)");
            builder.Property(x => x.Reason).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => x.BarberId);

            builder.HasIndex(x => new { x.AppointmentId, x.Reason })
                .IsUnique()
                .HasFilter("\"AppointmentId\" IS NOT NULL");

            builder.HasIndex(x => new { x.PaymentId, x.Reason })
                .IsUnique()
                .HasFilter("\"PaymentId\" IS NOT NULL");

            builder.HasOne<BarberDbModel>()
                .WithMany()
                .HasForeignKey(x => x.BarberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
