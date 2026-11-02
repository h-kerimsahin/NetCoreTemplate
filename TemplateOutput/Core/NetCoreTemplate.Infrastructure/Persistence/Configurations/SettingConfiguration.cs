using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Infrastructure.Persistence.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.Key, s.AppUserId }).IsUnique();

        builder.Property(s => s.Key).IsRequired().HasMaxLength(256);
        builder.Property(s => s.Value).HasMaxLength(4000);
        builder.Property(s => s.Description).HasMaxLength(1000);
    }
}
