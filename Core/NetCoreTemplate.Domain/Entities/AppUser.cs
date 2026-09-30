using NetCoreTemplate.Domain.Entities.Seedworks;

namespace NetCoreTemplate.Domain.Entities;

public class AppUser : BaseEntity
{
    public string UserName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    public bool EmailConfirmed { get; private set; } = false;
    public bool TwoFactorEnabled { get; private set; } = false;

    public int AccessFailedCount { get; private set; }
    public DateTime? LockoutEndDate { get; private set; }

    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString();

    public DateTime? LastLoginDate { get; private set; }

    public virtual AppUserProfile Profile { get; private set; } = null!;
}
