using System.ComponentModel.DataAnnotations;

namespace NetCoreTemplate.Domain.Enums;

public enum RoleType
{
    [Display(Name = "Süper Yönetici")] SuperAdmin = 1,
    [Display(Name = "Yönetici")] Admin = 2,
    [Display(Name = "Müdür")] Manager = 3,
    [Display(Name = "Destek")] Support = 4,
    [Display(Name = "Kullanıcı")] User = 5,
    [Display(Name = "Müşteri")] Customer = 6
}
