using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCoreTemplate.Domain.Entities;

namespace NetCoreTemplate.Infrastructure.Persistence.Configurations;

public class AppUserRefreshTokenConfiguration : IEntityTypeConfiguration<AppUserRefreshToken>
{
    public void Configure(EntityTypeBuilder<AppUserRefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.HasIndex(rt => rt.Token);

        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
        builder.Property(rt => rt.IpAddress).HasMaxLength(64);
    }
}
