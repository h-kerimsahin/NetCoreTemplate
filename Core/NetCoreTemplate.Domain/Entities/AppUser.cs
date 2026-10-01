using NetCoreTemplate.Domain.Entities.Seedworks;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Entities;

public class AppUser : BaseEntity
{
    public string UserName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool EmailConfirmed { get; private set; } = false;
    public bool TwoFactorEnabled { get; private set; } = false;
    public TwoFactorType TwoFactorType { get; private set; } = TwoFactorType.None;
    public int AccessFailedCount { get; private set; }
    public DateTime? LockoutEndDate { get; private set; }
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString();
    public DateTime? LastLoginDate { get; private set; }
    public virtual AppUserProfile Profile { get; private set; } = null!;
    public virtual ICollection<AppUserRole> UserRoles { get; private set; } = new HashSet<AppUserRole>();
    public virtual ICollection<AppUserRefreshToken> RefreshTokens { get; private set; } = new HashSet<AppUserRefreshToken>();
    public virtual ICollection<AppUserActivited> Activities { get; private set; } = new HashSet<AppUserActivited>();
    public virtual ICollection<Setting> Settings { get; private set; } = new HashSet<Setting>();

    private AppUser() { }

    public AppUser(string userName, string email, string passwordHash)
    {
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        Profile = new AppUserProfile(Id);
    }

    public bool IsLockedOut => LockoutEndDate.HasValue && LockoutEndDate.Value > DateTime.UtcNow;

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public void ConfirmEmail() => EmailConfirmed = true;

    public void EnableTwoFactor(TwoFactorType type)
    {
        TwoFactorEnabled = true;
        TwoFactorType = type;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorType = TwoFactorType.None;
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginDate = DateTime.UtcNow;
        AccessFailedCount = 0;
    }

    public void RecordFailedLogin(int maxFailedAttempts = 5, int lockoutMinutes = 15)
    {
        AccessFailedCount++;
        if (AccessFailedCount >= maxFailedAttempts)
        {
            LockoutEndDate = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    public void ResetAccessFailedCount() => AccessFailedCount = 0;

    public void RefreshSecurityStamp() => SecurityStamp = Guid.NewGuid().ToString();

    public void Unlock()
    {
        LockoutEndDate = null;
        AccessFailedCount = 0;
    }
}
