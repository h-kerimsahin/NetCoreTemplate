================================================================================
  NETCORE TEMPLATE v1.0 / v2.0 — PROJE README
  Çözüm Yolu: NetCoreTemplate.slnx  |  API: Presentation/NetCoreTemplate.Api
================================================================================

0. MIMARI & KATMANLAR (Clean Architecture, 4 Katman)
--------------------------------------------------------------------------------
  1) Core/NetCoreTemplate.Domain        → Entity, Enum, Arayüz (Interface), Value Object
  2) Core/NetCoreTemplate.Application   → DTO, CQRS (Feature=Command+Query), Service Interface, FluentValidation,
                                          Auditing (SaveChanges), Mapping (Mapster)
  3) Core/NetCoreTemplate.Infrastructure→ EF Core, IUnitOfWork + Repository, Hangfire, Serilog,
                                          LocalFileStorage/AzureBlob Storage, Email (SMTP),
                                          AppDbInitializer (Seed), OTel (OpenTelemetry)
  4) Presentation/NetCoreTemplate.Api   → Minimal API (Endpoints/*.cs), SignalR Hub, Middlewares,
                                          Swagger/Scalar/UI, Program.cs (Bootstrap)

  Teknik Yığın:
   - .NET 10 (min 8+), ASP.NET Core Web API (Minimal API), Entity Framework Core, SQL Server / LocalDB
   - MediatR 14 (Tek Paket), CQRS, FluentValidation, Mapster Mapping, Serilog (Structured Log)
   - JWT (Access 15dk / Refresh 7g), ASP.NET Core Identity (Custom User/Role/Permission RBAC Model)
   - Hangfire (Recurring Job, Dashboard, Memory Storage fallback)
   - OpenTelemetry 1.19.1 (Trace / Metric / OTLP Export)
   - SignalR (Realtime Notification Hub)
   - OData v4 ($filter, $select, $expand, $metadata - v1 & v2 versioned route)
   - Swagger UI + Scalar UI (OpenAPI v1/v2)
   - 3 Katmanlı Rate Limit (Auth 10/10dk, Global 100/1dk, IP 200/30dk)
   - X-Idempotency-Key (IdempotencyEndpointFilter) - Bütün POST/PUT'a takılı
   - Soft-Delete + Audit Entry (SaveChanges otomatik)
   - Data Protection Key, LocalFileStorage Path Traversal Koruması,
     CORS Secure By Default, Production SuperAdmin Seed Dev Only (güvenlik)

1. DEFAULT SEED BILGISI (SADECE Development Ortaminda)
--------------------------------------------------------------------------------
  Ilk deployda otomatik ayaga kalkar:
    * Roller (3): SuperAdmin | Admin | User
    * Permissionlar: Tum yetkiler AppPermission tablosuna seed edilir
    * ILK SUPERADMIN KULLANICI (Dev Ortami):
        Kullanici Adi: SuperAdmin
        E-posta     : superadmin@netcoretemplate.com
        Sifre       : Qwerty123!
        Rol         : SuperAdmin
  DIKKAT: Production ortaminda AppDbInitializer bu kullaniciyi OLUŞTURMAZ
         (AppDbInitializer.cs: IHostEnvironment.IsDevelopment() kontrolu).
         Production icin ilk SuperAdmin'i DB script veya CLI ile manuel yaratmalisin.

2. ILK CALISTIRMA (Development)
--------------------------------------------------------------------------------
  Terminal (Presentation/NetCoreTemplate.Api klasorunde):
    dotnet restore
    dotnet ef database update      (veya Package Manager Console: Update-Database)
    dotnet run
  Varsayilan URL (Properties/launchSettings.json):
    HTTP : http://localhost:5084
    HTTPS: https://localhost:7084
  Application URL (appsettings.json default): http://localhost:5084

3. TUM ENDPOINTLER — DETAYLI DOKUMAN
   Butun API'ler ApiResponse<T> formatinda doner:
     { statusCode, message, [data], [errors], [pageNumber], [pageSize], [totalCount] }
   Pagination query'leri varsayilan: ?page=1&size=20
   Idempotency: POST/PUT isteklerinde "X-Idempotency-Key: <string>" header'i ile
     ayni istek tekrar gonderildiginde tekrar execute edilmez (IdempotencyEndpointFilter).

================================================================================
  GRUP A: AUTHENDICATION & AUTHORIZATION (api/v1/auth)
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/AuthEndpoints.cs
================================================================================

Auth 1: Kullanici Girisi (Login)
  Method  : POST
  Route   : api/v1/auth/login
  Auth    : AllowAnonymous
  Rate    : AuthFixedWindowPolicy (10 istek / 10 dk, IP bazli)
  Idempotent: EVET (X-Idempotency-Key)
  Request (application/json, LoginRequestDto):
    {
      "emailOrUserName": "SuperAdmin",
      "password": "Qwerty123!",
      "rememberMe": true
    }
  Response (200 OK / 400 BadRequest / 401 Unauthorized / 403 Lockout):
    {
      "statusCode": 200,
      "message": "Giris basarili.",
      "data": {
        "accessToken": "<JWT Access Token>",
        "accessTokenExpiresAt": "2026-10-01T11:30:00Z",
        "refreshToken": "<Refresh Token GUID>",
        "refreshTokenExpiresAt": "2026-10-08T11:15:00Z",
        "user": { "id": "...", "userName": "SuperAdmin", "email": "...", ... }
      }
    }

Auth 2: Yeni Kullanici Kaydi (Register)
  Method  : POST
  Route   : api/v1/auth/register
  Auth    : AllowAnonymous
  Rate    : AuthFixedWindowPolicy
  Idempotent: EVET
  Request (application/json, RegisterRequestDto):
    {
      "userName": "john_doe",
      "email": "john@example.com",
      "password": "StrongP@ss1",
      "confirmPassword": "StrongP@ss1",
      "firstName": "John",
      "lastName": "Doe"
    }
  Response (201 Created / 400 Validation / 409 Conflict):
    { "statusCode": 201, "message": "Kullanici olusturuldu.", "data": { "id": "...", "userName": "john_doe", ... } }

Auth 3: Access Token Yenileme (Refresh Token)
  Method  : POST
  Route   : api/v1/auth/refresh
  Auth    : AllowAnonymous
  Idempotent: EVET
  Request: { "refreshToken": "<7-günlük refresh token>" }
  Response (200): Yeni { accessToken, refreshToken, expires } ciftini dondurur

Auth 4: Sifre Degistirme (Giris Yapan Kullanici)
  Method  : POST
  Route   : api/v1/auth/change-password
  Auth    : Bearer JWT (RequireAuthorization, herhangi bir role yeterli)
  Idempotent: EVET
  Request:
    {
      "currentPassword": "EskiSifre123!",
      "newPassword": "YeniKuvvetliSifre!1",
      "confirmNewPassword": "YeniKuvvetliSifre!1"
    }
  Response (200 OK / 400 / 404):
    { "statusCode": 200, "message": "Sifre degistirildi." }

Auth 5: E-Posta Doğrulama (Confirm Email)
  Method  : POST
  Route   : api/v1/auth/confirm-email
  Auth    : AllowAnonymous
  Idempotent: EVET
  Request (ConfirmEmailRequest):
    { "userId": "<GUID>", "token": "<EmailConfirmationToken>" }
  Response (200): { "message": "E-posta dogrulandi." }

Auth 6: Unuttugum Sifre - Token Talebi (Forgot Password)
  Method  : POST
  Route   : api/v1/auth/forgot-password
  Auth    : AllowAnonymous
  Idempotent: EVET
  Request (ForgotPasswordRequestDto): { "email": "kullanici@example.com" }
  Response (200): { "message": "Sifre sifirlama baglantisi e-posta ile gonderildi." }
  Note    : SMTP ayari dogru ise mail gider, aksi takdirde loga BASILIR (debug icin).

Auth 7: Sifre Sifirlama (Reset Password — Token ile)
  Method  : POST
  Route   : api/v1/auth/reset-password
  Auth    : AllowAnonymous
  Idempotent: EVET
  Request (ResetPasswordRequestDto):
    {
      "email": "kullanici@example.com",
      "token": "<FromForgotPasswordEmail>",
      "newPassword": "YeniSifre!2024",
      "confirmNewPassword": "YeniSifre!2024"
    }
  Response (200 / 400): { "message": "Sifre sifirlandi." }

Auth 8: 2FA Etkinlestir (Enable Two-Factor)
  Method  : POST
  Route   : api/v1/auth/enable-2fa
  Auth    : Bearer JWT
  Idempotent: EVET
  Request (EnableTwoFactorRequestDto): { "type": "Email" }  // Email/Sms/Totp enum
  Response (200): { "message": "2FA etkinlestirildi. Kod gonderildi." }

Auth 9: 2FA Dogrula / Giris (Verify Two-Factor)
  Method  : POST
  Route   : api/v1/auth/verify-2fa
  Auth    : AllowAnonymous
  Idempotent: EVET
  Request (VerifyTwoFactorRequestDto):
    { "emailOrUserName": "SuperAdmin", "code": "123456" }
  Response (200): Tam { accessToken, refreshToken } oturum bilgileri doner.

Auth 10: Tek Cihazdan Cikis (Logout — Refresh Token Revoke)
  Method  : POST
  Route   : api/v1/auth/logout
  Auth    : Bearer JWT
  Idempotent: EVET
  Request : Vucut yok. Header Authorization: Bearer <token> yeterli.
  Response (200): { "message": "Cikis yapildi. Refresh token iptal edildi." }

Auth 11: TUM CİHAZLARDAN CIKIS (Giris Yapan Kullanici - Kendisi)
  Method  : POST
  Route   : api/v1/auth/logout-all-devices
  Auth    : Bearer JWT
  Idempotent: EVET
  Request : Vucut yok
  Response (200):
    { "message": "Tum cihazlardan cikis yapildi." }
    Not: Kullanici SecurityStamp yenilenir + tum refresh token'lar DB'den revoke edilir

Auth 12: Ben Kimim (Current User Info)
  Method  : GET
  Route   : api/v1/auth/me
  Auth    : Bearer JWT
  Response (200):
    {
      "data": {
        "id": "<GUID>",
        "userName": "SuperAdmin",
        "email": "superadmin@netcoretemplate.com",
        "emailConfirmed": true,
        "twoFactorEnabled": false,
        "roles": ["SuperAdmin"],
        "permissions": ["*.*"],
        "profile": { "firstName": "...", "lastName": "...", ... }
      }
    }

================================================================================
  GRUP B: KULLANICI YONETIMI (api/v1/users-management)
  Gerekli Rol: Admin VEYA SuperAdmin  (Butun Route Grubunda)
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/UserManagementEndpoints.cs
================================================================================

UM 1: Kullaniciya Rol Ata (Assign Role) — Tum onceki rolleri silip yenilerini yazar
  Method  : POST
  Route   : api/v1/users-management/assign-role
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Request (RoleAssignRequestDto):
    {
      "userId": "<Kullanici GUID>",
      "roleIds": ["<Role1 GUID>", "<Role2 GUID>"]
    }
  Response (200): { "message": "Roller atandi." }
  Not: Atama sonrasi kullanici SecurityStamp yenilenir (JWT gecersiz olur, yeniden giris gerekir).

UM 2: Kullanıcıdan Rol Geri Al (Revoke Role)
  Method  : POST
  Route   : api/v1/users-management/revoke-role
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Request (RoleAssignRequestDto):
    {
      "userId": "<Kullanici GUID>",
      "roleIds": ["<Role1 GUID>"]   // Cikarilacak roller listesi
    }
  Response (200): { "message": "Roller geri alindi." }

UM 3: Admin Tarafindan — BIR KULLANICININ Tum Cihazlarindan Cikisi
  Method  : POST
  Route   : api/v1/users-management/{userId}/logout-all-devices
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Route Param: userId = GUID
  Response (200 / 404): { "message": "Tum cihazlardan cikis yapildi." }

UM 4: Kullanici Kilidini Ac (Unlock - Lockout sonrasi)
  Method  : POST
  Route   : api/v1/users-management/{userId}/unlock
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Route Param: userId = GUID
  Response (200): { "message": "Kullanici kilidi acildi." }
  Not: IdentityOptions.Lockout.Default (5 hatali giris = 5dk kilit)

UM 5: Tum Kullanicilari Sayfali Listele (Get All Users Paged)
  Method  : GET
  Route   : api/v1/users-management/paged?page=1&size=20
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Response (200, ApiResponse.Paged):
    {
      "pageNumber": 1,
      "pageSize": 20,
      "totalCount": 1234,
      "data": [
        { "id": "...", "userName": "...", "email": "...", "status": "Active", ... }
      ]
    }

================================================================================
  GRUP C: ROL & IZIN YONETIMI (api/v1/roles)
  Gerekli Rol: Admin VEYA SuperAdmin
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/RolesEndpoints.cs
================================================================================

R 1: Yeni Rol Olustur (Create Role)
  Method  : POST
  Route   : api/v1/roles
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Idempotent: EVET
  Request (RoleCreateRequest):
    { "name": "ProductManager", "description": "Urun yoneticisi, sadece urun CRUD yapar" }
  Response (201 Created):
    { "statusCode": 201, "message": "Rol olusturuldu.", "data": { "id": "<GUID>", "name": "ProductManager", ... } }

R 2: Rol Guncelle (Update Role)
  Method  : PUT
  Route   : api/v1/roles/{id:guid}
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Idempotent: EVET
  Request (RoleUpdateRequest): { "name": "Editor", "description": "Guncellenmis aciklama" }
  Response (200 / 404): Guncellenmis rol nesnesi doner

R 3: Rol Sil (Delete Role — Soft Delete)
  Method  : DELETE
  Route   : api/v1/roles/{id:guid}
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Response (200 / 404): { "message": "Rol silindi." }
  Not: Soft-Delete (Status=Deleted), veritabanindan silinmez, restore edilebilir.

R 4: Role Izinleri Ata (Assign Permission)
  Method  : POST
  Route   : api/v1/roles/{roleId}/permissions
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Idempotent: EVET
  Route Param: roleId = GUID
  Request (PermissionGrantDto):
    {
      "roleId": "<Rol GUID>",         // Route ile ayni olmalı, body'de referans icin
      "permissionIds": [
        "<Permission-Users.View>",
        "<Permission-Products.Manage>"
      ]
    }
  Not: Mevcut tum izinler silinip, YENI listedekiler yazilir (override).
  Response (200): { "message": "Izinler atandi." }

R 5: Rol Detay Getir (Get Role By Id)
  Method  : GET
  Route   : api/v1/roles/{id:guid}
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Response (200 / 404): Rol nesnesi + Permissions listesi

R 6: Tum Rolleri Sayfali Listele
  Method  : GET
  Route   : api/v1/roles?page=1&size=20
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Response (200, Paged): Status=Active olan rolleri listeler
  OData   : ODataQueryOptions<AppRole> zaten kullaniliyor, $filter/$orderby destek

================================================================================
  GRUP D: KULLANICI PROFILI (api/v1/user-profiles)
  Auth    : Tum Grub Bearer JWT (Login olan herkes)
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/UserProfileEndpoints.cs
================================================================================

UP 1: Kullanici Profilini ID'ye Gore Getir
  Method  : GET
  Route   : api/v1/user-profiles/{id:guid}
  Auth    : Bearer JWT
  Response (200):
    {
      "data": {
        "id": "<GUID>",
        "firstName": "Ahmet",
        "lastName": "Yilmaz",
        "birthDate": "1990-05-20T00:00:00Z",
        "phoneNumber": "+905551234567",
        "address": "...",
        "city": "Istanbul",
        "country": "Turkey",
        "avatarUrl": "/uploads/avatars/2026/09/30/abc.jpg",
        "bio": "Yazilimci"
      }
    }

UP 2: Tum Profilleri Sayfali Listele
  Method  : GET
  Route   : api/v1/user-profiles?page=1&size=20
  Auth    : Bearer JWT
  Response (200, Paged): Tüm kullanici profilleri

UP 3: KENDI Profilimi Guncelle
  Method  : PUT
  Route   : api/v1/user-profiles/mine
  Auth    : Bearer JWT (JWT icinden userId otomatik cekilir)
  Idempotent: EVET
  Request (UpdateUserProfileRequestDto):
    {
      "firstName": "Yeni Ahmet",
      "lastName": "Yeni Yilmaz",
      "birthDate": "1992-03-15T00:00:00Z",
      "phoneNumber": "+905329876543",
      "address": "Cadde 1",
      "city": "Ankara",
      "country": "Turkey",
      "avatarUrl": "/uploads/avatars/.../yeni.jpg",
      "bio": "Yeni bir bio"
    }
  Response (200 / 400 / 404): { "message": "Profil guncellendi." }

================================================================================
  GRUP E: DOSYA DEPOLAMA / UPLOAD (api/v1/file-storage)
  Auth    : Tum Grub Bearer JWT
  Izın Verilen Container (Whitelist, aksi durumda 400):
     avatars | documents | temp | exports
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/FileStorageEndpoints.cs
          Core/NetCoreTemplate.Infrastructure/Services/LocalFileStorageService.cs
================================================================================

FS 1: Dosya Yukle (Upload)
  Method  : POST
  Route   : api/v1/file-storage/upload
  Auth    : Bearer JWT
  Idempotent: EVET
  Content-Type: multipart/form-data  (DisableAntiforgery — form dosya icin)
  Request Form Alanlari (UploadFileRequest):
    file      : IFormFile  (zorunlu, max size appsettings FileStorage:MaxFileSizeBytes)
    container : "avatars"  (veya documents / temp / exports, Case-Insensitive)
  Response (201 Created):
    {
      "statusCode": 201,
      "message": "Dosya yüklendi.",
      "data": {
        "url": "/uploads/avatars/2026/10/01/abc123def456.png",
        "fileName": "<GUID>_original.png",
        "originalName": "profil.png",
        "size": 123456
      }
    }
  Not: Dosya yolu: wwwroot/uploads/{container}/{yyyy}/{MM}/{dd}/{Guid}.{ext} (Path Traversal KORUMALI)

FS 2: Dosya Sil (Delete)
  Method  : DELETE
  Route   : api/v1/file-storage/delete?filePath=/uploads/avatars/2026/10/01/abc.png
  Auth    : Bearer JWT
  Query   : filePath = upload endpointinden donen "url" degeri (relative path)
  Response (200 / 404):
    { "message": "Dosya silindi." }
  Not: Path Traversal korumasi var — _webRootPath disina cikamaz.

================================================================================
  GRUP F: BILDIRIMLER & SIGNALR (api/v1/notifications + Hub)
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/NotificationsEndpoints.cs
          Presentation/NetCoreTemplate.Api/Hubs/NotificationHub.cs
================================================================================

N 1: Bildirim Gonder (Admin/SuperAdmin) + SignalR Real-Time Push
  Method  : POST
  Route   : api/v1/notifications/send
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Idempotent: EVET
  Request (NotificationSendRequestDto):
    {
      "userId": "<Kullanici GUID veya NULL tum kullanicilar icin>",
      "title": "Yeni Bildirim",
      "message": "Profiliniz guncellendi.",
      "type": 0,                   // 0=Info, 1=Success, 2=Warning, 3=Error (NotificationType enum)
      "data": "{\"link\": \"/dashboard\"}"  // Opsiyonel, ekstra JSON veri
    }
  Response (201 Created): DB'ye yazilan AppNotification kaydini doner
  SignalR Yonlendirme:
    - userId DOLU ise:    hub.Clients.Group($"user-{{userId}}").SendAsync("ReceiveNotification")
    - userId BOS (NULL):  hub.Clients.All.SendAsync("ReceiveNotification")  // herkese yayin

N 2: BENIM Bildirimlerim (Paged)
  Method  : GET
  Route   : api/v1/notifications/mine?page=1&size=20
  Auth    : Bearer JWT
  Response (200, Paged): Kullanicinin kendi bildirimleri (Tarih DESC)

N 3: Tek Bildirim Okundu Isaretle
  Method  : POST
  Route   : api/v1/notifications/{notificationId}/mark-as-read
  Auth    : Bearer JWT
  Idempotent: EVET
  Response (200 / 404): { "message": "Bildirim okundu olarak isaretlendi." }

N 4: BENIM Tum Bildirimlerimi Okundu Isaretle
  Method  : POST
  Route   : api/v1/notifications/mark-all-as-read
  Auth    : Bearer JWT
  Idempotent: EVET
  Response (200): { "message": "47 bildirim okundu olarak isaretlendi." }

SIGNALR HUB:
  URL     : /hubs/notification
  Auth    : Bearer JWT (query string ?access_token=<JWT> veya Header)
  Client JS Ornek:
    const hub = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7084/hubs/notification?access_token=" + jwtToken)
        .build();
    hub.on("ReceiveNotification", (notif) => console.log("Bildirim!", notif));
    await hub.start();
    // Giris olunca otomatik kendi kullanici grubuna dahil olunur (NotificationHub.OnConnectedAsync)

================================================================================
  GRUP G: AUDIT GIRISLERI (Degisiklik Gecmisi) (api/v1/audit-entries)
  Gerekli Rol: Admin | SuperAdmin
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/AuditEntriesEndpoints.cs
================================================================================

A 1: TEK BIR ENTITY NIN DEGISIKLIK GECMISI
  Method  : GET
  Route   : api/v1/audit-entries/entity/{entityName}/{entityId}?page=1&size=20
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Route Param:
    entityName = Ornek: "AppUser", "AppRole", "AppUserProfile" (DbSet adi ile ayni)
    entityId   = GUID
  Response (200, Paged):
    {
      "pageNumber": 1,
      "data": [
        {
          "id": "...",
          "entityName": "AppUser",
          "entityId": "<userId GUID>",
          "changeType": "Modified",             // Added / Modified / Deleted / Restored
          "changedByUserId": "<Kim degistirdi GUID>",
          "changedAt": "2026-09-28T12:34:56Z",
          "oldValues": "{\"lastName\":\"Eski\"}",
          "newValues": "{\"lastName\":\"Yeni\"}"
        }
      ]
    }

A 2: BIR KULLANICININ YAPTIGI TUM DEGISIKLIKLER
  Method  : GET
  Route   : api/v1/audit-entries/user/{changedByUserId}?page=1&size=20
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Response (200, Paged): Yukaridaki yapida, Sadece degisikligi yapan userId filtresi

================================================================================
  GRUP H: SOFT DELETE - GERI YUKLEME (api/v1/soft-restore)
  Gerekli Rol: Admin | SuperAdmin
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/SoftRestoreEndpoints.cs
================================================================================

SR 1: Silinmis Kayitlari Listele (Entity Turune Gore)
  Method  : GET
  Route   : api/v1/soft-restore/deleted/{entityTypeName}?page=1&size=20
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Gecerli entityTypeName (Case Insensitive):
     "appuser"  | "user"    → AppUser tablosu (silinmis Status=Deleted olanlar)
     "approle"  | "role"    → AppRole tablosu
  Not: IgnoreQueryFilters() ile silinmis olanlar cekilir.
  Response (200, Paged): Silinmis kayitlar tam nesne olarak doner.

SR 2: Silinmis Veriyi Geri Yukle (Restore)
  Method  : POST
  Route   : api/v1/soft-restore/restore
  Auth    : Bearer JWT (Admin | SuperAdmin)
  Idempotent: EVET
  Request (RestoreEntityRequestDto):
    { "entityTypeName": "appuser", "entityId": "<Silinmis olan kaydin GUID>" }
  Response (200): { "message": "Veri geri yüklendi." }
  Not: Status = "Deleted" → "Active" olarak guncellenir, Audit kaydi atilir.

================================================================================
  GRUP I: SISTEM LOGLARI & KULLANICI AKTIVITELERI (api/v1/system-logs)
  Auth    : Tum Grub Bearer JWT
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/SystemLogsEndpoints.cs
================================================================================

SL 1: Sistem Loglarini Listele (Structured Serilog Write To DB)
  Method  : GET
  Route   : api/v1/system-logs?level=Error&page=1&size=20
  Auth    : Bearer JWT
  Query Secim:
    level = Bilgi | Debug | Information | Warning | Error | Fatal   (Opsiyonel, SystemLogLevel enum)
  Response (200, Paged<SystemLog>):
    {
      "data": [{
        "id": 123,
        "message": "Hangfire job basladi",
        "logLevel": "Information",
        "createdDate": "2026-10-01T08:00:00Z",
        "exception": null,
        "properties": "{\"MachineName\":\"KRMSHN\"}"
      }]
    }

SL 2: Kullanici Aktiviteleri (UserActivity) — Login/Logout/PasswordChange vb.
  Method  : GET
  Route   : api/v1/system-logs/activities?type=Login&page=1&size=20
  Auth    : Bearer JWT
  Query Secim:
    type = Login | Logout | FailedLogin | PasswordChange | ProfileUpdate |
           TwoFactorEnabled | TwoFactorVerified | EmailConfirmed | RoleAssigned
           (UserActivityType enum, Opsiyonel)
  Response (200, Paged<AppUserActivited>): Ne zaman, hangi IP, hangi tarayici, hangi userId

================================================================================
  GRUP J: HEALTH / METRICS / MONITORING (api/v1/health + Middleware)
  Auth    : AllowAnonymous (Hepsi)
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/HealthEndpoints.cs
          + Presentation/NetCoreTemplate.Api/DependencyInjection.cs (satir 220-253)
================================================================================

H 1: Liveness Probe (Basit Canlilik)
  Method  : GET
  Route   : api/v1/health/live      (Endpoint)
  Alternatif Ayni Is: /health/live  (Middleware'dan MapHealthChecks — Cloud Load Balancer icin)
  Response (200): { "message": "Liveness Healthy" }
  Kullanim: Kubernetes/Docker livenessProbe icin (sadece HTTP pipeline calisiyor mu diye bakar).

H 2: Readiness Probe (Servis Hazirlik — DB/SMTP/Hangfire baglantilari)
  Method  : GET
  Route   : api/v1/health/ready     (Endpoint, JSON ApiResponse format)
  Alternatif Ayni Is: /health/ready (Middleware, Duz Custom JSON ResponseWriter)
  Healthy   (200) : { "status": "Healthy", "totalDuration": "00:00:00.1234", "entries": {...} }
  Unhealthy (503) : { "statusCode": 503, "message": "Readiness Unhealthy",
                       "errors": ["smtp: Unhealthy", "db: Degraded"] }
  Kontrol Edilen Servisler (DependencyInjection.cs AddHealthChecks):
    - SQL Server / LocalDB (DB Baglantisi)
    - SMTP Server (Email SMTP Ping)
    - Hangfire Job Storage

H 3: Detayli Health Check (Tum Controllers Detay)
  Method  : GET
  Route   : api/v1/health/detailed
  Response (200):
    {
      "data": {
        "overallStatus": "Healthy",
        "totalDuration": "00:00:00.250",
        "checks": [
          { "name": "sqlserver", "status": "Healthy", "duration": "00:00:00.050", ... },
          { "name": "smtp", "status": "Healthy", "duration": "00:00:00.150", ... },
          { "name": "hangfire", "status": "Healthy", "duration": "00:00:00.050", ... }
        ]
      }
    }

H 4: Prometheus Scrape Endpoint Bilgilendirme
  Method  : GET
  Route   : api/v1/health/metrics
  Response (200): { "message": "Prometheus endpoint: /metrics" }

GERCEK PROMETHEUS METRIC ENDPOINT (Middleware uzerinden):
  Route   : /metrics
  Format  : text/plain (Prometheus exposition format)
  Ornek Metrikler: process_cpu_seconds_total, dotnet_total_memory_bytes,
                   http_requests_duration_seconds, custom_app_durations
  Kullanim: Prometheus scrape config icin target: http://<host>:5084/metrics

================================================================================
  GRUP K: V2 TEST (API Surumleme — v2 prefix)
  Auth    : AllowAnonymous
  Kaynak: Presentation/NetCoreTemplate.Api/Endpoints/V2TestEndpoints.cs
================================================================================

V2 1: v2 Surum Test Dummy Endpoint
  Method  : GET
  Route   : api/v2/test/dummy
  Response (200): { "data": { "version": "v2.0", "test": true }, "message": "V2 Dummy endpoint calisiyor." }
  Not: ApiVersioning ile Surum v1/v2 ayrı ayrı Swagger dokumanlari olusturulur.

================================================================================
  GRUP L: UI / DASHBOARD / DOKUMAN ENDPOINTLERI
  Tumunu Asagidakiler SADECE Development Ortaminda ACIKTIR (env.IsDevelopment())
================================================================================

L 1: Swagger UI (OpenAPI Explorer)
  URL         : /swagger
  Aciklama    : Swagger UI v1 & v2 beraber gorunur
  Endpointler :
    Swagger JSON v1: /swagger/v1/swagger.json
    Swagger JSON v2: /swagger/v2/swagger.json
    Alternatif JSON: /v1/openapi.json  ,  /v2/openapi.json  (MapSwagger custom format)
  Test Account:
    Authorize -> Bearer <JWT_TOKEN>  (Auth 1 Login'den aldigin accessToken)

L 2: Scalar UI (Modern, Guzel Gorunumlu API Dokuman)
  URL v1      : /scalar
  URL v2      : /scalar/v2
  Aciklama    : Scalar tarafindan modern, guzel bir OpenAPI UI (v1/v2 ayrı ayrı)

L 3: Hangfire Dashboard (Arka Plan Job Yoneticisi)
  URL         : /hangfire
  Auth        : HangfireAdminAuthorizationFilter → SADECE SuperAdmin Rolundekiler gorur
  OZELLIKLER  :
    - Kuyruktaki bekleyen / islenen / basarisiz isler
    - Tekrarlayan (Recurring) Job Listesi:
        * "nightly-cleanup" — her gun 02:00 UTC (AppDbContext Soft Delete temizligi, eski refresh token silme)
    - Servers, Retries, Scheduled, Queues sekmeleri
    - StatsPollingInterval = 2 sn

L 4: OData v4 Endpoint'leri ($metadata, $filter, $select, $orderby)
  URL v1  : /api/v1/odata/$metadata
  URL v2  : /api/v2/odata/$metadata
  Aciklama: Her iki surum icin EDM Model zaten kayitli (AppRole, AppPermission, AppUser, AppUserProfile Entity Set'leri)
  OData Query Ornek (Browser):
    /api/v1/odata/AppRole?$filter=startswith(Name,'Admin')&$select=Name,Id
    /api/v1/odata/AppUser?$orderby=UserName&$top=10&$count=true

L 5: Middleware'ler ve Guvenlik Katmanlari (Otomatik, Endpoint degil)
  * SecurityStampMiddleware  → Her istekte User'ın SecurityStamp'i DB'deki ile karsilastirir,
                               Degisti ise JWT gecersiz sayar (Logout All Devices mantigi)
  * IdempotencyEndpointFilter → POST/PUT'a X-Idempotency-Key ile tekrar karsisi
  * Rate Limiting 3 Katman   → AuthFixedWindow, GlobalSliding, IPConcurrencyLimiter
  * CorsSettings             → AllowCredentials = false (Secure By Default),
                               Admin App/Frontend icin CorsSettings.AllowedOrigins doldur
  * Serilog Request Logging  → Tum HTTP istekleri structured loglanir

================================================================================
4. PROJE CALISTIRMA (Adim Adim Quick Start)
================================================================================

  Adim 1: Veritabani Baglantisini Duzenle
    Presentation/NetCoreTemplate.Api/appsettings.json → ConnectionStrings:DefaultConnection
    (Default: LocalDB "MSSQLLocalDB", SQL Server Express / Normal SQL Server degistirebilirsiniz.)

  Adim 2: Migration Uygula (DB olustur)
    cd Presentation/NetCoreTemplate.Api
    dotnet ef database update

  Adim 3: Uygulamayi Calistir
    dotnet run

  Adim 4: Adresleri Ac
    Swagger UI     : http://localhost:5084/swagger
    Scalar v1      : http://localhost:5084/scalar
    Hangfire       : http://localhost:5084/hangfire   (SuperAdmin girisi ile)
    Health Ready   : http://localhost:5084/api/v1/health/ready

  Adim 5: Ilk Giris
    Email/User: SuperAdmin  |  Sifre: Qwerty123!  (Postman/Swagger Authorize'da Bearer token kullan)

================================================================================
5. YAYIN / PRODUCTION (ZORUNLU ADIMLAR)
================================================================================
  1. Presentation/NetCoreTemplate.Api/appsettings.Production.json → TUM
     "REPLACE_WITH_*" alanalrini doldur:
        - ConnectionStrings:DefaultConnection (Azure SQL / MSSQL Production)
        - JwtSettings:SecretKey (Minimum 64 karakter, RANDOM, User Secrets/Key Vault kullan)
        - AllowedHosts (Wildcard * KALDIRILDI: "yourdomain.com;www.yourdomain.com")
        - CorsSettings.AllowedOrigins (Frontend domainin)
        - EmailSettings (Production SMTP)
        - FileStorage.Provider = "Azure" (Azure Blob) veya "Local"
        - Hangfire.WorkerCount = Sunucu CPU cekirdegi sayisi
        - Serilog Sink ekle (MSSQL / Elasticsearch / Seq)
  2. JwtSettings.SecretKey ASLA appsettings.*.json'de HARDCODE YAPMA:
       Development: dotnet user-secrets init ; dotnet user-secrets set "JwtSettings:SecretKey" "<64+ char>"
       Production : Azure Key Vault / AWS Secrets Manager / Docker Secrets / ENV Degiskenleri
  3. SuperAdmin Seed SADECE Development calisir → Production'da ilk SuperAdmin'i manuel yarat
  4. Upload edilen dosyalar icin Azure Blob Storage'a gec (Provider=Azure daha saglam)
  5. Docker / Kubernetes icin health/ready endpointlerini K8S liveness/readiness prob olarak ata
  6. .gitignore zaten hassas dosyalari (appsettings.Staging, .env, secrets.json, sertifika key,
     upload dosyalar, loglar, .vscode/.idea, docker-compose.override vb.) %100 ignore eder.

================================================================================
6. KISA OZET — TOPLAM 35+ ENDPOINT
================================================================================
  Auth            : 12 adet (login, register, refresh, changePwd, confirmEmail, forgot/resetPwd,
                            2FA enable/verify, logout, logout-all, me)
  UserManagement  :  5 adet (assignRole, revokeRole, logoutAllDevices, unlock, paged list)
  Roles           :  6 adet (create, update, delete, assignPermission, getById, paged list)
  UserProfiles    :  3 adet (getById, paged list, updateMine)
  FileStorage     :  2 adet (upload, delete)
  Notifications   :  4 adet (send, mine list, markAsRead, markAllAsRead)  +  1 SignalR Hub
  AuditEntries    :  2 adet (byEntity, byUser)
  SoftRestore     :  2 adet (deletedList, restore)
  SystemLogs      :  2 adet (systemLogs paged, userActivities paged)
  Health          :  4 adet (live, ready, detailed, metrics info)  +  2 alternatif MW path
  V2 Test         :  1 adet (dummy)
  + UI Dashboard  :  Swagger(/swagger), Scalar(/scalar,/scalar/v2), Hangfire(/hangfire),
                     OData $metadata (v1/v2), Prometheus(/metrics)
  -------------------------
  TOPLAM API Endpoint : 43+ (Dashboard UI'lar haric 35+ REST)
================================================================================
