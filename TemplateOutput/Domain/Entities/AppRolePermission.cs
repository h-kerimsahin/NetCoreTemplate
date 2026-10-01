using $safeprojectname$.Domain.Entities.Seedworks;

namespace $safeprojectname$.Domain.Entities;

public class AppRolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public virtual AppRole Role { get; private set; } = null!;
    public virtual AppPermission Permission { get; private set; } = null!;

    private AppRolePermission() { }

    public AppRolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}
