using System.ComponentModel.DataAnnotations;

namespace $safeprojectname$.Domain.Enums;

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
    [Display(Name = "Hesap Kilitlendi")] AccountLocked = 16,
    [Display(Name = "Güvenlik Damgası Değişti")] SecurityStampChanged = 17,
    [Display(Name = "Rol Atandı")] RoleAssigned = 18,
    [Display(Name = "Rol Geri Alındı")] RoleRevoked = 19,
    [Display(Name = "İzin Verildi")] PermissionGranted = 20,
    [Display(Name = "Entity Geri Yüklendi")] EntityRestored = 21,
    [Display(Name = "Kullanıcı Şifre Değiştirdi")] UserPasswordChanged = 22,
    [Display(Name = "Bildirim Gönderildi")] NotificationSent = 23,
    [Display(Name = "Dosya Yüklendi")] FileUploaded = 24,
    [Display(Name = "Dosya Silindi")] FileDeleted = 25
}
