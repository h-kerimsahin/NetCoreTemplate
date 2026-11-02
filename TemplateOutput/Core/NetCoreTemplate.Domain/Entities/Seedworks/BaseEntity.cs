using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Entities.Seedworks;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    public string? Slug { get; set; }

    public DateTime CreatedDate { get; protected set; }

    public DateTime? ModifiedDate { get; protected set; }

    public DateTime? DeletedDate { get; protected set; }

    public Guid? CreatedBy { get; protected set; }

    public Guid? ModifiedBy { get; protected set; }

    public Guid? DeletedBy { get; protected set; }

    public EntityStatus Status { get; protected set; } = EntityStatus.Active;

    public bool IsActive => Status == EntityStatus.Active;

    public bool IsDeleted => Status == EntityStatus.Deleted;

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    public virtual void Activate(Guid? userId = null)
    {
        Status = EntityStatus.Active;
        ModifiedBy = userId;
        ModifiedDate = DateTime.UtcNow;
    }

    public virtual void Deactivate(Guid? userId = null)
    {
        Status = EntityStatus.Passive;
        ModifiedBy = userId;
        ModifiedDate = DateTime.UtcNow;
    }

    public virtual void Archive(Guid? userId = null)
    {
        Status = EntityStatus.Archived;
        ModifiedBy = userId;
        ModifiedDate = DateTime.UtcNow;
    }

    public virtual void Delete(Guid? userId = null)
    {
        Status = EntityStatus.Deleted;
        DeletedBy = userId;
        DeletedDate = DateTime.UtcNow;
        ModifiedBy = userId;
        ModifiedDate = DateTime.UtcNow;
    }

    public virtual void Restore(Guid? restoredByUserId = null)
    {
        Status = EntityStatus.Active;
        DeletedDate = null;
        DeletedBy = null;
        ModifiedDate = DateTime.UtcNow;
        ModifiedBy = restoredByUserId;
    }
}
