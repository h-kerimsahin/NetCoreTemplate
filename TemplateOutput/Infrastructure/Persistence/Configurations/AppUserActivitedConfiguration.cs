using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Infrastructure.Persistence.Configurations;

public class AppUserActivitedConfiguration : IEntityTypeConfiguration<AppUserActivited>
{
    public void Configure(EntityTypeBuilder<AppUserActivited> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => a.AppUserId);
        builder.HasIndex(a => a.ActivityType);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });

        builder.Property(a => a.Description).IsRequired().HasMaxLength(4000);
        builder.Property(a => a.EntityName).HasMaxLength(100);
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.UserAgent).HasMaxLength(1000);
    }
}
