using $safeprojectname$.Domain.Entities.Seedworks;

namespace $safeprojectname$.Domain.Entities;

public class Setting : BaseEntity
{
    public string Key { get; private set; } = null!;
    public string? Value { get; private set; }
    public string? Description { get; private set; }
    public bool IsSensitive { get; private set; }
    public Guid? AppUserId { get; private set; }
    public virtual AppUser? AppUser { get; private set; }

    private Setting() { }

    public Setting(string key, string? value, string? description = null, bool isSensitive = false, Guid? appUserId = null)
    {
        Key = key;
        Value = value;
        Description = description;
        IsSensitive = isSensitive;
        AppUserId = appUserId;
    }

    public void Update(string? value, string? description = null)
    {
        Value = value;
        if (description != null) Description = description;
    }
}
