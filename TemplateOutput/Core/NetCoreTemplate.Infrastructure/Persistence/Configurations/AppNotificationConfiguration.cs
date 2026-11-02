using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Infrastructure.Persistence.Configurations;

public class AppNotificationConfiguration : IEntityTypeConfiguration<AppNotification>
{
    public void Configure(EntityTypeBuilder<AppNotification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.HasIndex(n => new { n.UserId, n.IsRead });

        builder.Property(n => n.Title).IsRequired().HasMaxLength(256);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(4000);
        builder.Property(n => n.NotificationType).IsRequired();
        builder.Property(n => n.IsRead).IsRequired();
        builder.Property(n => n.Data).HasMaxLength(4000);
        builder.Property(n => n.CreatedAt).IsRequired();
    }
}
