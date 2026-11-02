using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Infrastructure.Persistence.Configurations;

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.HasKey(sl => sl.Id);
        builder.HasIndex(sl => sl.LogLevel);
        builder.HasIndex(sl => sl.UserId);
        builder.HasIndex(sl => sl.CreatedDate);

        builder.Property(sl => sl.Message).IsRequired().HasMaxLength(4000);
        builder.Property(sl => sl.ExceptionType).HasMaxLength(512);
        builder.Property(sl => sl.StackTrace).HasMaxLength(8000);
        builder.Property(sl => sl.Source).HasMaxLength(500);
        builder.Property(sl => sl.RequestPath).HasMaxLength(1000);
        builder.Property(sl => sl.RequestMethod).HasMaxLength(10);
        builder.Property(sl => sl.IpAddress).HasMaxLength(64);
    }
}
