using $safeprojectname$.Domain.Entities.Seedworks;
using $safeprojectname$.Domain.Enums;

namespace $safeprojectname$.Domain.Entities;

public class AppPermission : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }
    public PermissionGroup GroupName { get; private set; }
    public bool IsActive { get; private set; } = true;
    public virtual ICollection<AppRolePermission> RolePermissions { get; private set; } = new HashSet<AppRolePermission>();

    private AppPermission() { }

    public static AppPermission Create(string name, string code, PermissionGroup groupName, string? description = null, bool isActive = true)
    {
        return new AppPermission
        {
            Name = name,
            Code = code,
            GroupName = groupName,
            Description = description,
            IsActive = isActive
        };
    }

    public void Update(string name, string code, PermissionGroup groupName, string? description = null)
    {
        Name = name;
        Code = code;
        GroupName = groupName;
        Description = description;
    }

    public void ActivatePermission() => IsActive = true;
    public void DeactivatePermission() => IsActive = false;
}
