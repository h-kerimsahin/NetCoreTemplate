using $safeprojectname$.Domain.Entities.Seedworks;

namespace $safeprojectname$.Domain.Entities;

public class AppUserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public virtual AppUser User { get; private set; } = null!;
    public virtual AppRole Role { get; private set; } = null!;

    private AppUserRole() { }

    public AppUserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
