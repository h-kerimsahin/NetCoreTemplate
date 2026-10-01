using System.ComponentModel.DataAnnotations;

namespace $safeprojectname$.Domain.Enums;

public enum JobStatus
{
    [Display(Name = "Bekliyor")] Pending = 1,
    [Display(Name = "İşleniyor")] Processing = 2,
    [Display(Name = "Başarılı")] Succeeded = 3,
    [Display(Name = "Başarısız")] Failed = 4,
    [Display(Name = "Tekrar Deneniyor")] Retry = 5,
    [Display(Name = "İptal Edildi")] Cancelled = 6
}
