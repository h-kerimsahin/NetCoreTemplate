using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using $safeprojectname$.Domain.Entities;

namespace $safeprojectname$.Infrastructure.Persistence.Configurations;

public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.ChangedByUserId);

        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(256);
        builder.Property(a => a.EntityId).IsRequired();
        builder.Property(a => a.PropertyName).IsRequired().HasMaxLength(256);
        builder.Property(a => a.OldValue).HasMaxLength(4000);
        builder.Property(a => a.NewValue).HasMaxLength(4000);
        builder.Property(a => a.ChangedAt).IsRequired();
        builder.Property(a => a.EntityState).IsRequired();
    }
}
