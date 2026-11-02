using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Infrastructure.Persistence.Configurations;

public class AppPermissionConfiguration : IEntityTypeConfiguration<AppPermission>
{
    public void Configure(EntityTypeBuilder<AppPermission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(256);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(256);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.GroupName).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
