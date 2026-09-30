using System.ComponentModel.DataAnnotations;

namespace NetCoreTemplate.Domain.Enums;

public enum EntityStatus
{
    [Display(Name = "Aktif")]
    Active = 1,

    [Display(Name = "Pasif")]
    Passive = 2,

    [Display(Name = "Silinmiş")]
    Deleted = 3,

    [Display(Name = "Arşivlenmiş")]
    Archived = 4
}