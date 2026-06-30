using BarberOS.Infrastructure.Persistence.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberOS.Infrastructure.Persistence.Configurations
{

    public class UserConfiguration : IEntityTypeConfiguration<UserDbModel>
    {
        public void Configure(EntityTypeBuilder<UserDbModel> builder)
        {
            builder.ToTable("users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email).IsRequired().HasMaxLength(160);
            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
            builder.Property(x => x.FullName).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Phone).HasMaxLength(30);
            builder.Property(x => x.PhotoUrl).HasMaxLength(2048);
            builder.Property(x => x.Role).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired();
        }
    }
}
