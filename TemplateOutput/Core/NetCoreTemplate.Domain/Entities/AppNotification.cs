using NetCoreTemplate.Domain.Entities.Seedworks;
using NetCoreTemplate.Domain.Enums;

namespace NetCoreTemplate.Domain.Entities;

public class AppNotification : BaseEntity
{
    public Guid? UserId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public NotificationType NotificationType { get; private set; }
    public bool IsRead { get; private set; } = false;
    public DateTime? ReadAt { get; private set; }
    public string? Data { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AppNotification() { }

    public static AppNotification Create(Guid? userId, string title, string message, NotificationType notificationType, string? data = null)
    {
        return new AppNotification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = notificationType,
            Data = data,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
