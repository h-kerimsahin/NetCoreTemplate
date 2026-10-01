using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Infrastructure.Persistence.Configurations;

public class BackgroundJobLogConfiguration : IEntityTypeConfiguration<BackgroundJobLog>
{
    public void Configure(EntityTypeBuilder<BackgroundJobLog> builder)
    {
        builder.HasKey(j => j.Id);
        builder.HasIndex(j => new { j.JobType, j.CreatedDate });

        builder.Property(j => j.JobType).IsRequired().HasMaxLength(256);
        builder.Property(j => j.JobStatus).IsRequired();
        builder.Property(j => j.Payload).HasMaxLength(4000);
        builder.Property(j => j.ErrorMessage).HasMaxLength(4000);
        builder.Property(j => j.RetryCount).IsRequired();
    }
}
