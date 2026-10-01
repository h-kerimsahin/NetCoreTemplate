using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.UserName).IsUnique();

        builder.Property(u => u.UserName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(64);

        builder.HasOne(u => u.Profile)
            .WithOne(p => p.AppUser)
            .HasForeignKey<AppUserProfile>(p => p.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.AppUser)
            .HasForeignKey(rt => rt.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Activities)
            .WithOne(a => a.AppUser)
            .HasForeignKey(a => a.AppUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.Settings)
            .WithOne(s => s.AppUser)
            .HasForeignKey(s => s.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
