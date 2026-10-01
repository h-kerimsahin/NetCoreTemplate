using System.ComponentModel.DataAnnotations;

namespace $safeprojectname$.Domain.Enums;

public enum NotificationType
{
    [Display(Name = "Bilgi")] Info = 1,
    [Display(Name = "Başarılı")] Success = 2,
    [Display(Name = "Uyarı")] Warning = 3,
    [Display(Name = "Hata")] Error = 4,
    [Display(Name = "Sistem")] System = 5
}
