using System.ComponentModel.DataAnnotations;

namespace NetCoreTemplate.Domain.Enums;

public enum UserActivityType
{
    [Display(Name = "Kayıt Olundu")] Register = 1,
    [Display(Name = "Giriş Yapıldı")] Login = 2,
    [Display(Name = "Çıkış Yapıldı")] Logout = 3,
    [Display(Name = "Giriş Başarısız")] LoginFailed = 4,
    [Display(Name = "Şifre Değiştirildi")] PasswordChanged = 5,
    [Display(Name = "Şifre Sıfırlama İsteği")] ForgotPasswordRequested = 6,
    [Display(Name = "Şifre Sıfırlandı")] PasswordReset = 7,
    [Display(Name = "Profil Güncellendi")] ProfileUpdated = 8,
    [Display(Name = "Email Doğrulandı")] EmailConfirmed = 9,
    [Display(Name = "2FA Aktifleştirildi")] TwoFactorEnabled = 10,
    [Display(Name = "2FA Doğrulandı")] TwoFactorVerified = 11,
    [Display(Name = "2FA Kapatıldı")] TwoFactorDisabled = 12,
    [Display(Name = "Entity Oluşturuldu")] EntityCreated = 13,
    [Display(Name = "Entity Güncellendi")] EntityUpdated = 14,
    [Display(Name = "Entity Silindi")] EntityDeleted = 15,
    [Display(Name = "Hesap Kilitlendi")] AccountLocked = 16
}
