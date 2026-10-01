using System.ComponentModel.DataAnnotations;

namespace $safeprojectname$.Domain.Enums;

public enum PermissionGroup
{
    [Display(Name = "Yetkilendirme")] Auth = 1,
    [Display(Name = "Kullanıcı Yönetimi")] UserManagement = 2,
    [Display(Name = "Roller")] Roles = 3,
    [Display(Name = "Sistem Logları")] SystemLogs = 4,
    [Display(Name = "Denetim")] Audit = 5,
    [Display(Name = "Bildirimler")] Notifications = 6,
    [Display(Name = "Raporlar")] Reports = 7,
    [Display(Name = "Geri Yükleme")] Restore = 8,
    [Display(Name = "Depolama")] Storage = 9,
    [Display(Name = "Hangfire")] Hangfire = 10,
    [Display(Name = "Dosya")] File = 11,
    [Display(Name = "Ayarlar")] Settings = 12
}
