using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Infrastructure.Persistence.Configurations;

public class AppUserProfileConfiguration : IEntityTypeConfiguration<AppUserProfile>
{
    public void Configure(EntityTypeBuilder<AppUserProfile> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.AppUserId).IsUnique();

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.PhoneNumber).HasMaxLength(20);
        builder.Property(p => p.City).HasMaxLength(50);
        builder.Property(p => p.Country).HasMaxLength(50);
        builder.Property(p => p.AvatarUrl).HasMaxLength(500);
        builder.Property(p => p.Bio).HasMaxLength(2000);
    }
}
