using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{
    public class BarberConfiguration : IEntityTypeConfiguration<BarberDbModel>
    {
        public void Configure(EntityTypeBuilder<BarberDbModel> builder)
        {
            builder.ToTable("barbers");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId).IsRequired();
            builder.HasIndex(x => x.UserId).IsUnique();

            builder.Property(x => x.BarbershopId).IsRequired();
            builder.HasIndex(x => x.BarbershopId);

            builder.Property(x => x.LunchStart).IsRequired();
            builder.Property(x => x.LunchEnd).IsRequired();
            builder.Property(x => x.AvailableDays).IsRequired();

            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();

            builder.HasOne<UserDbModel>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<BarbershopDbModel>()
                .WithMany()
                .HasForeignKey(x => x.BarbershopId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
