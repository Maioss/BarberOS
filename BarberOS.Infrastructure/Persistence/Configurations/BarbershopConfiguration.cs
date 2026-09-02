using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{
    public class BarbershopConfiguration : IEntityTypeConfiguration<BarbershopDbModel>
    {
        public void Configure(EntityTypeBuilder<BarbershopDbModel> builder)
        {
            builder.ToTable("barbershops");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
            builder.Property(b => b.Address).IsRequired().HasMaxLength(300);
            builder.Property(b => b.City).IsRequired().HasMaxLength(100);
            builder.Property(b => b.Phone).HasMaxLength(20);

            builder.Property(b => b.TimeZoneId)
                .IsRequired()
                .HasMaxLength(64)
                .HasDefaultValue("America/Bogota");

            builder.HasOne(b => b.Parent)
                .WithMany(b => b.Branches)
                .HasForeignKey(b => b.ParentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }
}
