using NetCoreTemplate.Domain.Entities.Seedworks;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Entities;

public class AppUserRefreshToken : BaseEntity
{
    public Guid AppUserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public TokenType TokenType { get; private set; }
    public string? IpAddress { get; private set; }
    public virtual AppUser AppUser { get; private set; } = null!;

    private AppUserRefreshToken() { }

    public AppUserRefreshToken(Guid appUserId, string token, DateTime expiresAt, TokenType tokenType, string? ipAddress = null)
    {
        AppUserId = appUserId;
        Token = token;
        ExpiresAt = expiresAt;
        TokenType = tokenType;
        IpAddress = ipAddress;
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
