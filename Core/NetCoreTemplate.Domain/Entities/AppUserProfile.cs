using NetCoreTemplate.Domain.Entities.Seedworks;

namespace NetCoreTemplate.Domain.Entities;

public class AppUserProfile : BaseEntity
{
    public Guid AppUserId { get; private set; }

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;

    public DateOnly? BirthDate { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? Address { get; private set; }

    public string? City { get; private set; }

    public string? Country { get; private set; }

    public string? AvatarUrl { get; private set; }

    public string? Bio { get; private set; }

    public virtual AppUser AppUser { get; private set; } = null!;
    public virtual AppUserActivited AppUserActivited { get; private set; } = null!;
    public virtual AppUserRefreshToken AppUserRefreshToken { get; private set; } = null!;
    public virtual Setting AppUserSetting { get; private set; } = null!;
}
