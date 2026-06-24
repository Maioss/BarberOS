using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<ServiceDbModel>
    {
        public void Configure(EntityTypeBuilder<ServiceDbModel> builder)
        {
            builder.ToTable("services");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BarbershopId).IsRequired();
            builder.HasIndex(x => x.BarbershopId);

            builder.Property(x => x.Name).IsRequired().HasMaxLength(80);
            builder.Property(x => x.Description).HasMaxLength(300);

            builder.Property(x => x.Price)
                .IsRequired()
                .HasColumnType("numeric(12,2)");

            builder.Property(x => x.DurationMinutes).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();

            builder.HasOne<BarbershopDbModel>()
                .WithMany()
                .HasForeignKey(x => x.BarbershopId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
