using $safeprojectname$.Domain.Entities.Seedworks;

namespace $safeprojectname$.Domain.Entities;

public class AppRole : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public string? Description { get; private set; }
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();
    public virtual ICollection<AppUserRole> UserRoles { get; private set; } = new HashSet<AppUserRole>();
    public virtual ICollection<AppRolePermission> RolePermissions { get; private set; } = new HashSet<AppRolePermission>();

    private AppRole() { }

    public static AppRole Create(string name, string? description = null)
    {
        return new AppRole
        {
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            Description = description,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    public void UpdateDetails(string name, string? description = null)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }
}
