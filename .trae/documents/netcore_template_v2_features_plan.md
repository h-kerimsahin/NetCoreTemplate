# NetCoreTemplate v2 — 14 Özellik Kapsamlı Implementation Plan

## Repository Research (Önceki Yapı + Ön Hazır Bulgular)

**Mevcut Mimari:** 4 Katmanlı Clean Arch. Tek yönlü referans: Api → Infrastructure → Application → Domain. Domain 0 ProjectRef (SADECE EF Core NuGet). Build 0 Hata / 0 Uyarı. Program.cs 22 satır (≤25).

**ÖNEMLİ BULGULAR (Kullanıcının istediği özelliklerden KISIMEN ZATEN VAR olanlar):**
1. **Hatalı Login Denemesi (Madde 3):**
   - AppUser.cs `AccessFailedCount`/`LockoutEndDate`/`IsLockedOut`/`RecordFailedLogin(maxAttempts=5, lockoutMin=15)` **halihazırda mevcut**
   - LoginCommandHandler zaten `user.IsLockedOut` kontrolü + `RecordFailedLogin()` çağırıyor
   - **Eksik:** Sabit "5 deneme / 15 dk" değerleri hardcode. Bunları PasswordSettings POCO + appsettings ile KONFİGÜRE EDİLEBİLİR yapmak + Kaynak bazlı (IP/User) kombinasyon lockout.
2. **Güçlü Şifre Policy (Madde 5):**
   - `RegisterCommandValidator.cs` zaten `Matches(@"[A-Z]").Matches(@"[a-z]").Matches(@"[0-9]")` + MinLength(8) uyguluyor
   - **Eksik:** Özel karakter (!@#$%^&*) regex EKSİK + ResetPassword validator'ında aynı kurallar YOK + PasswordStrength enum/score helper EKSİK + Configurable min complexity POCO
3. **Audit Trail Detaylı (Madde 11):**
   - AppDbContext.ProcessAudit() içinde `EntityState.Modified` dalında zaten tüm property değişiklikleri `Dictionary<string, {Original, Current}>` ile `changes` dict'e toplanıp **JSON serialize edilerek** AppUserActivited.Description alanına yazılıyor
   - **Eksik:** Bu değişiklikler AppUserActivited tablosunda JSON string olarak saklandığı için **QUERY edilemez (LINQ ile arama/sıralama yapılamaz)**. Ayrıca AuditEntry ÖZEL tablosu + her bir property değişikliği için ayrı satır (EntityName, EntityId, PropertyName, OldValue, NewValue, ChangedByUserId, ChangedAt) + CreatedBy/ModifiedBy/DeletedBy tam doldurulması (şu an IHttpContextAccessor'dan alınmıyor) + Soft Restore audit'i EKSİK
4. **SecurityStamp (Madde 7):**
   - `AppUser.SecurityStamp` string GUID property'si var, `UpdatePassword()` metodu içinde **şifre değiştirince zaten yenileniyor**
   - **Eksik:** JWT token içine security_stamp claim'i EKLENMİYOR → JWT doğrulama sonrası middleware'da `DB SecurityStamp == JWT Claim` karşılaştırması YOK → "Tüm cihazlardan çıkış" (RegenerateSecurityStamp Command) endpointi YOK + LogoutAllDevices handler EKSİK + Blacklist mekanizması EKSİK

---

## Kapsam (14 Özellik — Kullanıcının İsteği)

Kullanıcı **şunları** istedi (sırayla, numaraları eşleştim):
1. ✅ RBAC (Rol & Yetki Sistemi) — Madde 1
2. ✅ Rate Limiting — Madde 2
3. ✅ Hatalı Login Denemesi Sınırı + CORS Sıkılaştırma — Madde 3 (Kullanıcı login limit ekledi)
4. ✅ Health Checks — Madde 4
5. ✅ Güçlü Şifre Policy — Madde 5
6. ✅ SecurityStamp + Tüm Cihazlardan Çıkış — Madde 7 (Madde 6 (Redis) istemedi)
7. ✅ Hangfire Background Jobs + Email Kuyruk — Madde 8
8. ✅ File Storage (IFileStorageService + 2 impl) — Madde 10
9. ✅ API Versioning + Audit Trail Detaylı — Madde 11
10. ✅ OpenTelemetry (Metrics + Traces) — Madde 13
11. ✅ Soft Delete Geri Alma (Restore) — Madde 15
12. ✅ SignalR Bildirim (Notification Hub + INotificationService) — Madde 17
13. ✅ OData v4 Desteği (Endpoint'lerde $filter/$select/$orderby/$expand/$count) — EKSTRA (son istek)
14. ✅ CreatedBy/ModifiedBy/DeletedBy Otomatik Doldurma (IHttpContextAccessor → SaveChanges → BaseEntity audit alanları) — ZATEN ProcessAudit var ama tam çalışmıyor, bu plan içinde tamamlancak

---

## Files and Modules (Değişecek / Yeni Dosyalar)

### A. Katman Başına Yeni Dosyalar

#### 📌 DOMAIN (NetCoreTemplate.Domain)
**YENİ Entity/Enum/Interface (13 dosya):**
- `Entities/AppRole.cs` — Role entity (Id, Name, NormalizedName, Description, NormalizedDescription, ConcurrencyStamp)
- `Entities/AppUserRole.cs` — Junction (UserId+RoleId PK, FK)
- `Entities/AppPermission.cs` — Claim bazlı ince yetki (Id, Name, Code, Description, GroupName)
- `Entities/AppRolePermission.cs` — Junction (RoleId+PermissionId)
- `Entities/AuditEntry.cs` — Detaylı kolon değişikliği (Id, EntityName, EntityId, PropertyName, OldValue, NewValue, ChangedByUserId, ChangedAt, EntityState byte)
- `Entities/AppNotification.cs` — Bildirim entity (Id, UserId? (nullsa herkese gönder), Title, Message, Type, IsRead, ReadAt, Data JSON, CreatedAt)
- `Entities/BackgroundJobLog.cs` — Hangfire job log + Email queue log (Id, JobType, JobStatus, Payload JSON, ErrorMessage, RetryCount, StartedAt, FinishedAt)
- `Enums/RoleType.cs` — SuperAdmin / Admin / Manager / Support / User / Customer (6 enum)
- `Enums/PermissionGroup.cs` — Auth / UserManagement / Roles / SystemLogs / Audit / Notifications / Reports (8 enum)
- `Enums/NotificationType.cs` — Info / Success / Warning / Error / System (5 enum)
- `Enums/JobStatus.cs` — Pending / Processing / Succeeded / Failed / Retry / Cancelled (6 enum)
- `Interfaces/Services/IFileStorageService.cs` — UploadFile/DeleteFile/FileExists/GetFileInfo metotları
- `Interfaces/Services/INotificationService.cs` — SendToUser / SendToRole / SendToAll / SendToConnectionsAsync
- `Interfaces/Repositories/IAppRoleRepository.cs`, `IAppUserRoleRepository.cs`, `IAppPermissionRepository.cs`, `IAppRolePermissionRepository.cs`, `IAuditEntryRepository.cs`, `IAppNotificationRepository.cs`, `IBackgroundJobLogRepository.cs` — Yeni 7 Repository interface

---

#### 📌 APPLICATION (NetCoreTemplate.Application)
**YENİ CQRS + DTO + Settings + SecurityStampMiddleware (40+ dosya, kısaca özet):**
- `DTOs/Settings/LockoutSettings.cs` — MaxFailedAttempts (default 5), LockoutMinutes (default 15), ResetFailedCountMinutes (default 30)
- `DTOs/Settings/PasswordSettings.cs` — MinLength (8), RequireUppercase (true), RequireLowercase (true), RequireDigit (true), RequireSpecialChar (true), SpecialChars (@"!@#$%^&*()_+\-=\[\]{};':\\|,.<>\/?")
- `DTOs/Common/RoleAssignRequestDto.cs`, `PermissionGrantDto.cs`, `NotificationDto.cs`, `AuditEntryDto.cs`, `RestoreEntityRequestDto.cs`, `FileUploadResultDto.cs`
- `DTOs/Common/PasswordStrength.cs` enum + PasswordStrengthCalculator static helper
- `Security/SecurityStampMiddleware.cs` — JWT doğrulama sonrası claim'deki security_stamp ile DB'deki AppUser.SecurityStamp karşılaştırması (uyuşmazsa 401)
- `Features/Roles/*` — 4 Command (Create/Update/Delete/AssignPermission) + 2 Query (GetAll/GetById)
- `Features/UserManagement/*` — AssignRole Command, RevokeRole Command, LogoutAllDevices Command (= SecurityStamp regenerate et)
- `Features/AuditEntries/*` — GetAuditTrailForEntity PagedQuery (OData uyumlu: EntityName, EntityId, ChangedByUserId, DateRange filtreleri) + GetAuditForUser
- `Features/Notifications/*` — Send Command, GetUserNotifications PagedQuery, MarkAsRead Command, SignalR Hub NotificationsEndpoints ile eşle
- `Features/SoftRestore/*` — RestoreDeletedEntity Command (Status=Deleted → Active, restore edilen kullanıcı Required Authorization + Admin role)
- `Features/FileStorage/*` — UploadFile Command (IFormFile, containerName) + DeleteFile Command (fileUrl)
- `Validators/UpdatePasswordValidator.cs` (strong password rule) + ResetPassword Validator rewrite + Custom Validator base: StrongPasswordValidator<T> (shared rules)
- `DependencyInjection.cs` — PasswordSettings + LockoutSettings Configure; Application katmanına hangi servislerin DI'a ekleneceği

---

#### 📌 INFRASTRUCTURE (NetCoreTemplate.Infrastructure)
**YENİ Implementation + Configuration + Repository (16+ dosya):**
- `Services/LocalFileStorageService.cs` — Local wwwroot/{container} impl (Development)
- `Services/AzureBlobStorageService.cs` — Azure Blob Storage impl (Production) - interface
- `Services/InProcessNotificationService.cs` — INotificationService + SignalR IHubContext<NotificationHub> enjekte et + Hub üzerinden client'a anında bildirim gönder + DB'ye AppNotification kaydet
- `Services/HangfireJobScheduler.cs` — IBackgroundJobScheduler interface (Schedule/Enqueue/Recurring) + Hangfire Schedule tanımları
- `Jobs/EmailSenderJob.cs` — Hangfire Job: IEmailService.SendEmailAsync çağır + Polly Retry Policy (3 deneme, 30sn aralık) + başarısızsa BackgroundJobLog Failed olarak kaydet + Sonraki gün tekrar deneme
- `Jobs/RecurringJobs.cs` — NightlyCleanupJob (30 günlük eski SystemLogları arşivle / hard delete; 1 yıldır Lockout olan kullanıcıları kilidi aç, eski RefreshTokenları temizle) + AuditLogArchiver
- `Persistence/Configurations/*` — AppRoleConfiguration, AppUserRoleConfiguration (composite PK), AppPermissionConfiguration, AuditEntryConfiguration (Index: EntityName+EntityId), AppNotificationConfiguration (Index: UserId+IsRead), BackgroundJobLogConfiguration
- `Persistence/Repositories/AppRoleRepository.cs` + diğer 6 yeni repository
- `DependencyInjection.cs` — Hangfire AddHangfire / AddHangfireServer + UseSqlServerStorage (DB'de Hangfire tabloları); PasswordSettings + LockoutSettings Configure; IFileStorageService AddKeyed: Keyed "Local" / "AzureBlob"; INotificationService Scoped; OpenTelemetry AddOpenTelemetry / Metrics / Traces AddAspNetCore / AddEntityFrameworkCore / AddHttpClient Instrumentation; Polly Policy Register (AsyncRetryPolicy for Email/External APIs)

---

#### 📌 PRESENTATION (NetCoreTemplate.Api)
**YENİ Middleware + Endpoint + Hub + Rate Limit + CORS + Health + ApiVersioning + OData (25+ dosya):**
- `Middlewares/RateLimitingConfiguration.cs` (3 Policy):
  - 🔒 **LoginRateLimitPolicy:** FixedWindow (10 dakika, 10 istek) — /api/auth/login, /api/auth/refresh, /api/auth/forgot-password, /api/auth/reset-password, /api/auth/verify-2fa
  - 👤 **PerUserTokenRateLimitPolicy:** SlidingWindow (60 saniye, 150 istek) — Tüm Authorize endpointler; token'dan userId alınır, key = $"{userId}:{method}:{path}"
  - 🌐 **IpSlidingWindowPolicy:** SlidingWindow (15 dakika, 500 istek) — Global IP bazlı koruma
- `Middlewares/CorsPolicyConfiguration.cs` — CorsSettings POCO (AllowedOrigins string[], AllowAllInDevelopment=true); Development AllowAll; Production WithOrigins(AllowedOrigins) + AllowCredentials + WithExposedHeaders("X-Pagination,X-Idempotency-Key")
- `Health/DatabaseHealthCheck.cs`, `SmtpHealthCheck.cs`, `FileStorageHealthCheck.cs`, `HangfireHealthCheck.cs` — IHealthCheck 4 tane + HealthCheckResponseWriter (ApiResponse formatında /health endpoint)
- `OpenTelemetry/OTelConfiguration.cs` — AddOpenTelemetry(): Tracing (AspNetCore + EF Core + HttpClient + Hangfire Source name), Metrics (AspNetCore + HttpClient + EventCounters), UseOtlpExporter (appsettings OTLP endpoint configurable)
- `ApiVersioning/VersioningConfiguration.cs` — AddApiVersioning (1.0 default, AssumeDefaultVersionWhenUnspecified=true, ReportApiVersions=true), AddMvc()+AddOData() + OData EDM Model (OData v4)
- `OData/ODataModelBuilder.cs` — EdmModel oluştur: AppUser, AppUserProfile, AppRole, SystemLog, AppUserActivited, AuditEntry, AppNotification entity'leri EDM Model'e dahil; her entity için EntitySet + Function (GetUserRoles, GetUserNotifications) + ODataRoutingMetadata
- `Hubs/NotificationHub.cs` — SignalR Hub (OnConnectedAsync, OnDisconnectedAsync, SendToUser, SendToAll) + Strongly Typed Hub Interface IClientNotificationHub
- `Endpoints/RolesEndpoints.cs` (7 CRUD + AssignPermission), `Endpoints/UserManagementEndpoints.cs` (AssignRole + RevokeRole + LogoutAllDevices + GetAllUsers Paged OData)
- `Endpoints/AuditEntriesEndpoints.cs` (2 Paged OData endpoint), `Endpoints/NotificationsEndpoints.cs` (Send/MarkAsRead/GetUserNotifications + SignalR Hub map endpoint /hubs/notification)
- `Endpoints/FileStorageEndpoints.cs` (Upload/Delete + DisableRequestSizeLimit + [Authorize] + Antiforgery + Form file)
- `Endpoints/SystemLogsEndpoints.cs` — Mevcut 2 endpoint OData v4 destekli hale getir ($filter/$orderby/$select/$count)
- `Endpoints/SoftRestoreEndpoints.cs` (RestoreDeletedEntity + GetDeletedEntities Paged OData + Admin policy)
- `Endpoints/HealthEndpoints.cs` — /health/live (200 boş), /health/ready (DB+Smtp+FileStorage+Hangfire), /health/metrics (Prometheus format)
- `DependencyInjection.cs` — BÜTÜNÜ toplar: RateLimiter Policy'leri + CORS sıkı + Authentication (JWT SecurityStamp claim dahil) + Authorization (Role ve Permission Policy Provider + DefaultPolicy RequireAuthenticatedUser) + HealthChecks + Hangfire + ApiVersioning + OData + SignalR + OpenTelemetry + Swagger (Security Definition + Auth Button + OData OperationFilter) + Scalar UI (v1 ve v2 sürümleri ayrı sekme)
- `Program.cs` — ConfigurePipeline İÇİNDE sıra: UseHttpsRedirection → UseSerilogRequestLogging → UseCors → UseRateLimiter → UseMiddleware<SecurityStampMiddleware> → UseAuthentication → UseAuthorization → UseSwagger/UI → MapScalar → MapHealthChecks → MapHub<NotificationHub>("/hubs/notification") → MapControllers → MapEndpoints → app.UseHangfireDashboard("/hangfire", DashboardOptions (RequireAuthorization("AdminPolicy")))
- `appsettings.json` — 10+ yeni section: LockoutSettings, PasswordSettings, CorsSettings, RateLimiting, HealthChecks, OpenTelemetry (OtlpEndpoint), FileStorage (Provider: Local/AzureBlob, LocalPath, AzureBlobConnStr), Hangfire (QueueName, Dashboard), NotificationSettings (SignalR EnableKeepAlive, MessageTtlSeconds), ApiVersioning (DefaultVersion=1.0), OData (MaxTop=100, EnableNoDollarSign=true, EnableExpand=true, EnableSelect=true, EnableFilter=true, EnableOrderBy=true, EnableCount=true, MaxExpansionDepth=5, MaxAnyAllExpressionDepth=3)

---

## Implementation Steps (BAĞIMLILIK SIRALI — Adım 1 yapmadan 2. adım çalışmaz)

### Aşama 0: Paket Kurulumu (İLK — TÜM NUGET'LER)
```
Domain: hiç paket yok (zaten EF 11-preview)
Application: 0 Yeni paket (FluentValidation/MediatR/AutoMapper zaten var)
Infrastructure:
  Hangfire.Core (1.8.x LTS)
  Hangfire.SqlServer (1.8.x)
  Hangfire.AspNetCore (1.8.x)
  Polly (8.4.x) (Retry/CircuitBreaker)
  Microsoft.Extensions.Caching.StackExchangeRedis (Madde 6 yapsak da yapmasak da SecurityStamp blacklist için hazır)
  Azure.Storage.Blobs (12.22.x) (Azure Blob FileStorage impl)
  OpenTelemetry (1.10.x)
  OpenTelemetry.Extensions.Hosting (1.9.x)
  OpenTelemetry.Instrumentation.AspNetCore (1.9.x)
  OpenTelemetry.Instrumentation.EntityFrameworkCore (1.0.0-beta.x)
  OpenTelemetry.Exporter.OpenTelemetryProtocol (1.9.x)
  OpenTelemetry.Exporter.Console (dev için)
Api:
  Microsoft.AspNetCore.RateLimiting (built-in .NET 11, 0 paket? Check — 11 preview'da built-in FrameworkReference gelir)
  Asp.Versioning.Http (8.1.x)
  Asp.Versioning.OData (8.1.x) (OData v4!)
  Microsoft.AspNetCore.OData (8.2.x) — Ya da Asp.Versioning.OData kullan (ikisi aynı şeyi yapar, versiyonlamayı Asp yapar)
  Microsoft.AspNetCore.SignalR.Core (FrameworkReference 11'de var, Microsoft.AspNetCore.App içinde)
  OpenTelemetry.Instrumentation.AspNetCore (ekstra ekle)
  Swashbuckle.AspNetCore.Filters (Swagger Security Definitions için)
  Swashbuckle.AspNetCore.Annotations
```
**Adım 0.1:** Tüm csproj dosyalarına bu paketleri Net11 preview uyumlu EN SON SÜRÜM ekle. `dotnet restore` ilk çalıştır. Hata yoksa Aşama 1'e geç.

---

### Aşama 1: Entity + Repository + EF Config (Katman: Domain → Infrastructure)
**Sıra ile:**
1. `AppRole.cs` + `AppUserRole.cs` + `AppPermission.cs` + `AppRolePermission.cs` 4 entity'yi Domain.Entities içinde yaz. `RoleType` + `PermissionGroup` enum'larını Domain.Enums'a ekle. `AppUser` entity'sine 2 yeni navigation property ekle: `public virtual ICollection<AppUserRole> UserRoles { get; set; } = new HashSet<AppUserRole>();`
2. `AuditEntry.cs` entity'si (kolon değişiklikleri için ÖZEL tablo) + `AppNotification.cs` + `BackgroundJobLog.cs` 3 entity daha ekle. Enum'lar: `NotificationType`, `JobStatus`.
3. 7 tane yeni Domain Repository Interface yaz (IAppRole/IAppUserRole/IAppPermission/IAppRolePermission/IAuditEntry/IAppNotification/IBackgroundJobLog)
4. AppDbContext.cs 7 yeni DbSet ekle (AppRoles, AppUserRoles, AppPermissions, AppRolePermissions, AuditEntries, AppNotifications, BackgroundJobLogs)
5. **CreatedBy/ModifiedBy/DeletedBy OTOMATİK DOLDURMA DÜZELT:** `AppDbContext` constructor'ına `IHttpContextAccessor` enjekte et, `private readonly IHttpContextAccessor _httpContextAccessor;` ProcessAudit() içinde: `var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;` Sonra createdBy/modifiedBy/deletedBy'ı **KESİN** bu claim'deki Guid ile doldur (şu an entry.Property CurrentValue var ama HttpContext'ten çekilmediği için boş!)
6. ProcessAudit **TAMAMLAMA** (Kolon bazlı AuditEntry tablosuna yaz): Mevcut JSON description ile birlikte YENİ `List<AuditEntry> auditEntries = new();` oluştur. Her Modified entry içinde değişen her prop için `new AuditEntry(entityName, entityId, propName, originalStr, currentStr, userId, now)` 1 satır oluştur. AuditEntry listesini de `AddRange(auditEntries)` ile kaydet. (Not: AppUserActivited JSON'u koru, AuditEntry yeni query edilebilir detay tablo)
7. Soft Restore metodu: `BaseEntity`'ye `public void Restore(Guid? restoredByUserId)` metodu ekle → `Status = EntityStatus.Active; DeletedDate=null; DeletedBy=null; ModifiedDate=now; ModifiedBy=restoredByUserId;`
8. 7 Repository Implementation + UnitOfWork'a 7 yeni Lazy property ekle (UnitOfWork.cs): `public IAppRoleRepository AppRoles => LazyGet(ref _appRoles, services);` ve diğer 6
9. `IAppDbContext.cs` interface içinde 7 yeni DbSet property ekle
10. 7 EntityConfiguration (Fluent API) yaz (AppRoleConfiguration Name UNIQUE index, AppUserRole composite PK UserId+RoleId, AuditEntry EntityName+EntityId non-clustered index, AppNotification UserId+IsRead index vb.)

---

### Aşama 2: Ayarlar + DTO + Validator Güçlendirme (Katman: Application)
1. `PasswordSettings.cs` + `LockoutSettings.cs` 2 DTO Settings sınıfı yaz (Application/DTOs/Settings klasör)
2. Strong Password Policy: `PasswordStrength` enum (TooShort/Weak/Medium/Strong/VeryStrong) + `PasswordStrengthCalculator.Calculate(string password)` static helper yaz (skorlama: 0-5 puan, her şart 1 puan: uzunluk ≥12, büyük harf, küçük harf, sayı, özel karakter, benzersiz 8+ karakter)
3. Mevcut `RegisterCommandValidator.cs` DÜZELT: Özel karakter regex EKLE (Matches(PasswordSettings.RequireSpecialChar!)), ayrıca `Property` yerine `IRuleBuilderOptions` ile StrongPasswordValidator<T> BASE class yaz → Register, ResetPassword, ChangePassword validator'ları aynı kodu KALITIMLA paylaşsın (kod tekrarını kaldır)
4. `ResetPasswordCommandValidator` MEVCUT şu an `NotEmpty().MinLength(8)` → TAMAMI StrongPasswordValidator'dan KALITIM alsın.
5. `DTOs/Common` içine 10 ortak DTO ekle: RoleAssignRequestDto, RoleDto, PermissionDto, NotificationDto, AuditEntryDto, RestoreEntityRequestDto, FileUploadResultDto, NotificationSendRequestDto, AssignPermissionDto, LogoutAllDevicesDto
6. `SecurityStampMiddleware` Application katmanında yaz: `RequestDelegate next`, `IUnitOfWork unitOfWork` constructor'a enjekte, InvokeAsync içinde: eğer User identity authenticated değilse next; değilse userId claim'den al, user'ı DB'den GetByIdAsync ile al; user == null ise 401; user.SecurityStamp ile JWT'de "security_stamp" claim karşılaştır; eşleşmezse 401 (SecurityStamp uyuşmazlığı, kullanıcı tüm cihazlardan çıkış yapmış veya şifre değiştirmiş). Eşleşirse next() devam et.

---

### Aşama 3: JWT Claim'e SecurityStamp + Token Service GÜNCELLEME (Katman: Infrastructure)
1. `JwtTokenService.cs` GÜNCELLE: `GenerateAccessToken(AppUser user)` içinde Claim listesine YENİ `new Claim("security_stamp", user.SecurityStamp)` EKLE (JWT içinde artık security stamp var)
2. `ITokenService.cs` interface'de GenerateAccessToken imzası değişmiyor zaten (AppUser parametresi alıyor)
3. Yeni `Services/HangfireJobScheduler.cs` + `IBackgroundJobScheduler` interface Domain katmanında: EnqueueEmailJob, ScheduleRecurringCleanup, etc.

---

### Aşama 4: Application CQRS 8 Yeni Feature Klasörü (Katman: Application)
Her biri için: Command/Query + Validator + Handler (TÜMÜ `ApiResponse<T>`/`ApiResponse<bool>`/`PagedResponse<T>` kullan)
1. `Features/Roles` (6 CQRS): CreateRoleCommand/Handler, UpdateRoleCommand, DeleteRoleCommand, GetRoleByIdQuery, GetAllRolesPagedQuery, AssignPermissionToRoleCommand
2. `Features/UserManagement` (5 CQRS): AssignRoleToUserCommand/Handler (Admin yetkisi gerek), RevokeRoleFromUserCommand, GetAllUsersPagedQuery, LogoutAllDevicesCommand (= user.SecurityStamp = Guid.NewGuid().ToString() + SaveChanges + Activity Log → SecurityStampChanged UserActivityType Enum'a EKLE) + UnlockUserCommand (LockoutEndDate=null, AccessFailedCount=0)
3. `Features/AuditEntries` (2 CQRS Paged OData uyumlu): GetAuditByEntityIdPagedQuery (entityName, entityId, pageNumber, pageSize) + GetAuditByUserIdPagedQuery
4. `Features/Notifications` (4 CQRS): SendNotificationCommand (Admin) + GetMyNotificationsPagedQuery (OData) + MarkNotificationAsReadCommand + MarkAllAsReadCommand
5. `Features/SoftRestore` (2 CQRS): GetDeletedEntitiesPagedQuery (EntityStatus=Deleted'i ignore etmesi için `IQueryable.IgnoreQueryFilters().Where(...)` çağır — ÖNEMLİ) + RestoreDeletedEntityCommand (entityType, entityId → reflection ile bul, Restore() metodunu çağır, SaveChanges + AuditEntry "Restored" kaydet)
6. `Features/FileStorage` (2 CQRS): UploadFileCommand (IFormFile, ContainerName) → ApiResponse<FileUploadResultDto> (fileUrl, sizeBytes, contentType) + DeleteFileCommand
7. `Features/Auth` YENİ 2 endpoint: ChangePasswordCommand (currentPassword, newPassword) + ConfirmEmailCommand (userId, token)
8. `UserActivityType` enum GENİŞLET: SecurityStampChanged (LogoutAllDevices logu), RoleAssigned, RoleRevoked, PermissionGranted, EntityRestored, PasswordChanged, NotificationSent, FileUploaded, FileDeleted

---

### Aşama 5: File Storage + Notification Service Implementasyonları (Katman: Infrastructure)
1. `IFileStorageService.cs` Domain interface: 4 metod — `UploadFileAsync`, `DeleteFileAsync`, `FileExistsAsync`, `GetPublicUrlAsync`
2. `LocalFileStorageService` (Development): wwwroot/uploads/{containerName}/{yyyy}/{MM}/{dd}/{randomGuid_extName} dosyası yaz, URL `$"{appBaseUrl}/uploads/{containerName}/..."` (Production CDN ise AzureBlob)
3. `AzureBlobStorageService`: Container oluştur, BlobClient Upload, Delete
4. DI Registration DependencyInjection: IConfiguration FileStorage:Provider → "AzureBlob" ise AzureBlob, Local ise LocalFileStorage scoped
5. `INotificationService`: `SendToUserAsync(Guid userId, AppNotification notification, CancellationToken)` içinde 2 işlem: (a) DB'ye AppNotification kaydet (b) SignalR HubContext Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notificationDto)
6. `NotificationHub.cs` Api'de: `IHubContext<NotificationHub>` → Infrastructure NotificationService içinde kullanılır. OnConnectedAsync: Hub kullanıcı ConnectionId tutan ConcurrentDictionary.

---

### Aşama 6: Hangfire + Email Kuyruk + Polly Retry (Katman: Infrastructure → Api)
1. `HangfireDependencyInjection`: Infrastructure DependencyInjection içinde `services.AddHangfire(c => c.SetDataCompatibilityLevel(CompatibilityLevel.Version_180).UseSimpleAssemblyNameTypeSerializer().UseRecommendedSerializerSettings().UseSqlServerStorage(connectionString, new SqlServerStorageOptions { CommandBatchMaxTimeout = TimeSpan.FromMinutes(5), SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5), QueuePollInterval = TimeSpan.Zero, UseRecommendedIsolationLevel = true, DisableGlobalLocks = true, PrepareSchemaIfNecessary = true, SchemaName = "Hangfire" }));`
2. `EmailSenderJob.cs`: `public class EmailSenderJob(IEmailService emailService, IBackgroundJobLogRepository jobLogRepo, IUnitOfWork uow) { public async Task Execute(Guid jobId, string toEmail, string subject, string body, bool isHtml, CancellationToken ct) { job = jobLogRepo.GetById(jobId); try { Policy.Handle<SmtpCommandException>().Or<IOException>().WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))).ExecuteAsync(async () => await emailService.SendEmailAsync(toEmail, subject, body, isHtml, ct)); job.Status = JobStatus.Succeeded; job.FinishedAt = now; } catch(Exception ex) { job.Status = JobStatus.Failed; job.ErrorMessage = ex.ToString(); job.RetryCount = 3; } await uow.SaveChangesAsync(ct); } }`
3. TÜM Mevcut Email ÇAĞRILARI (ForgotPasswordHandler.SendResetPasswordEmail, RegisterHandler.WelcomeEmail, EnableTwoFactorHandler.Email 2FA code, VerifyTwoFactor.SendEmail değil) → Direkt `await emailService.SendEmailAsync()` YERINE: (a) yeni BackgroundJobLog(pending, "EmailSenderJob", JSON payload = new {toEmail, subject, body}) INSERT (b) `BackgroundJob.Enqueue<EmailSenderJob>(job => job.Execute(job.Id, toEmail, subject, body, isHtml, CancellationToken.None))` → Artık Email'ler HTTP request içinde BEKLEMEZ, kuyrukta asenkron çalışır! Başarısız olursa 2 dk sonra tekrar dener. UI DONDURMAZ.
4. NightlyCleanupJob (RecurringJob): Her gün 00:00 çalışır — (1) CreatedDate < 30 gün önce SystemLog hard delete (IgnoreQueryFilters + ExecuteDelete), (2) ExpireDate < şimdi olan RefreshTokenlar sil, (3) LockoutEndDate < şimdi -24 saat olan kullanıcıların AccessFailedCount sıfırla
5. Infrastructure DependencyInjection.AddInfrastructureServices sonuna: `services.AddHangfireServer(options => { options.WorkerCount = Environment.ProcessorCount * 5; options.Queues = new[] { "default", "email", "notifications", "recurring" }; });` + `RecurringJob.AddOrUpdate<NightlyCleanupJob>("nightly-cleanup", job => job.Execute(CancellationToken.None), Cron.Daily(0, 0), TimeZoneInfo.Local);`

---

### Aşama 7: Api Katmanı — Middlewares + Pipeline + Endpointler (Katman: Presentation)
SIRA İLE:
1. `CorsSettings.cs` POCO + `CorsPolicyConfiguration`: `appsettings.CorsSettings.AllowedOrigins` Production'da WithOrigins() ile, Development'ta AllowAll
2. RateLimiting: 3 Policy (FixedWindow auth, Sliding perUser, Global IpSliding) yaz; ConfigureServices içinde `builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter(AuthFixedWindowPolicy) options.AddSlidingWindowLimiter(...))`; ConfigurePipeline UseRateLimiter UseCors'dan SONRA, UseAuthentication'dan ÖNCE olmalı
3. HealthChecks: ConfigureServices `AddHealthChecks().AddCheck<DbHealthCheck>("sql-db", HealthStatus.Unhealthy, new[] {"ready", "db"}).AddCheck<SmtpHealthCheck>("smtp").AddCheck<FileStorageHealthCheck>("storage").AddCheck<HangfireHealthCheck>("hangfire")`; MapHealthChecks /health/live (boş 200), /health/ready (tüm checks), /health/detailed (JSON format, ApiResponse)
4. OpenTelemetry: ConfigureServices AddOpenTelemetry → WithTracing (AddAspNetCoreInstrumentation, AddEntityFrameworkCoreInstrumentation, AddHangfireInstrumentation, AddHttpClientInstrumentation, AddOtlpExporter → appsettings OTLP endpoint, Development'ta Console Exporter). WithMetrics (AspNetCore, EventCounters, HttpClient, AddPrometheusExporter)
5. API Versioning + OData v4:
   - ConfigureServices: `services.AddApiVersioning(o => o.DefaultApiVersion = new ApiVersion(1, 0); o.AssumeDefaultVersionWhenUnspecified = true; o.ReportApiVersions = true;).AddMvc().AddOData(opt => { opt.AddRouteComponents("api/v{version:apiVersion}", ODataModelBuilder.GetEdmModel()); opt.Count().Filter().OrderBy().Expand().Select().SetMaxTop(100); opt.EnableNoDollarSign = true; })`
   - Endpoint'ler artık `api/v1/auth`, `api/v1/user-profiles` (version route prefix). MapGroup içinde `HasApiVersion(1.0)`
   - ODataModelBuilder.GetEdmModel(): EntitySet<AppUser>("Users"), EntitySet<AppRole>("Roles"), EntitySet<AppNotification>("Notifications"), EntitySet<AuditEntry>("AuditEntries"), EntitySet<SystemLog>("SystemLogs"), EntitySet<AppUserActivited>("UserActivities") hepsini ekle; ayrıca `Function("SendEmail").Returns<bool>()` gibi function/action tanımla
   - 6 Paged Query endpoint (GetAllUsers, GetDeleted, AuditByEntity, SystemLogs, UserActivities, Notifications) → `[EnableQuery]` attribute + ODataOptions DI + _uow.Repositories.GetAll().AsQueryable().ApplyODataQuery(queryOptions) — OData sonuçların ApiResponse/PagedResponse içine SAR (toplam satır sayısı OData total count'tan al)
6. SignalR Notification Hub: ConfigureServices `AddSignalR()`; ConfigurePipeline `app.MapHub<NotificationHub>("/hubs/notification")`. Hub içinde OnConnectedAsync: userId claim'den `Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}")`
7. **8 Yeni Endpoint Class:**
   - RolesEndpoints (CRUD + AssignPermission) — Admin rol zorunlu
   - UserManagementEndpoints (AssignRole/RevokeRole/LogoutAll/Unlock/GetAllUsers OData) — Admin
   - AuditEntriesEndpoints (GetByEntityId/GetByUserId OData)
   - NotificationsEndpoints (Send/MarkAsRead/MarkAllAsRead/GetMyNotifications OData)
   - SoftRestoreEndpoints (GetDeletedList OData / RestoreDeleted) — Admin
   - FileStorageEndpoints (Upload IFormFile / Delete) — Authorize
   - HealthEndpoints (Liveness/Readiness/Detailed/Prometheus)
   - Versiyon V2 Dummy Endpoints (ApiVersioning doğrulama için api/v2/test)
8. **MEVCUT Endpoint GÜNCELLEMELER:**
   - AuthEndpoints: Login → RateLimit "AuthFixedWindowPolicy" RequireRateLimiting; Register → RateLimit; LogoutAllDevices yeni endpoint ekle; ChangePassword endpoint ekle; ConfirmEmail endpoint ekle; HasApiVersion(1.0) + group route prefix = "api/v{version:apiVersion}/auth"
   - UserProfileEndpoints: Version prefix 1.0 + OData $select desteği
   - SystemLogsEndpoints: 2 endpoint OData $filter/$orderby/$select/$count + [EnableQuery] + PagedResponse
9. Swagger + Scalar Güncelleme:
   - SwaggerGen: AddSecurityDefinition("Bearer", OpenApiSecurityScheme JWT Bearer); AddSecurityRequirement; EnableAnnotations; DocumentFilter 1.0 ve 2.0 için ayrı SwaggerDoc
   - Scalar: MapScalarApiReference(c => c.Title = $"NetCoreTemplate API v{apiVersion}") her sürüm için ayrı sekme
10. Hangfire Dashboard: ConfigurePipeline'ın SONUNA (UseAuthentication SONRASI): `app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = new[] { new HangfireAdminAuthorizationFilter() }, IgnoreAntiforgeryToken = true, DashboardTitle = "NetCoreTemplate Hangfire", StatsPollingInterval = 2000 });` HangfireAdminAuthorizationFilter: HttpContext.User.Claims Role==SuperAdmin veya Admin ise izin ver, değil 403.
11. Program.cs SATIR SAYISI ≤25 KORU (mevcut 22). ConfigureServices/ConfigurePipeline static sınıflarda olmalı, Program içinde sadece builder + Build + Run olmalı. Gerekirse Hangfire RecurringJob başlatma kısmını ayrı static class'a çek.
12. appsettings.json TÜM section'ları EKLE: LockoutSettings, PasswordSettings, CorsSettings, RateLimiting, HealthChecks (true/false), OpenTelemetry (OtlpEndpoint), FileStorage (Provider="Local"), Hangfire (Enabled=true, DashboardTitle, QueueName="default"), NotificationSettings (SignalRMessageTtl=86400), ApiVersioning, OData

---

### Aşama 8: Entity Framework Migration + Seed Data
1. `dotnet ef migrations add V2_FullFeatures_AllEntities --output-dir Persistence/Migrations --startup-project ../Presentation/NetCoreTemplate.Api`
2. Migration içinde: Hangfire için ayrı schema `Hangfire` (UseSqlServerStorage config'te var, otomatik oluşur). Migration Snapshot'ı güncelle.
3. Seed Data sınıfı: `Persistence/SeedData/AppDbInitializer.cs` — 6 tane default Role (SuperAdmin/Admin/Manager/Support/User/Customer), 50+ Permission (Auth.*, Users.*, Roles.*, Audit.*, Notifications.*, Logs.*, Restore.*, Storage.*, Hangfire.*) + SuperAdmin role tüm permissionları atar, seed 1 user SuperAdmin@netcoretemplate.com, şifre Qwerty123! + rol ataması.
4. DependencyInjection AddInfrastructureServices sonunda: `app.ApplicationServices.EnsureDatabaseMigratedAndSeeded()` extension metodu (migrate pending ise UpdateDatabase, sonra SeedData.Initialize)

---

### Aşama 9: AI_RULES_FOR_THIS_REPO.md GÜNCELLEMESİ (EN SON ADIM)
Tüm yeni özellikler için KURAL dosyasını güncelle:
- 7 YENİ KURAL EKLE (K14: RBAC Kullanımı, K15: Rate Limit Zorunluluğu, K16: Email Hangfire Kuyruk Kullanımı, K17: AuditEntry Query et, K18: OData kullanım kılavuzu, K19: SignalR Bildirim pattern, K20: FileStorage interface kullan)
- 12 YENİ Anti-pattern EKLE (A24: Direkt SmtpClient.SendAsync çağır — Hangfire Job kullan; A25: CreatedBy null bırak — IHttpContextAccessor'dan al; A26: SecurityStamp kontrolünü middleware'dan geçme — atlama; A27: Endpoint'te AllowAll CORS; A28: OData yerine custom filter code — $filter kullan; vb.)
- Feature Checklist 17→37 maddeye çıkart (her yeni özellik için 2-3 check)
- 8 Adım Yeni Entity Ekleme Rehberine: Adım 9 = OData EDM EntitySet ekle, Adım 10 = SeedData, Adım 11 = Hangfire Dashboard yetkilendirmesi
- **TEK KURAL MD KORU:** Başka MD OLUŞTURMA, sadece AI_RULES_FOR_THIS_REPO.md güncelle

---

## Dependencies and Considerations
1. **Paket Uyumluluğu:** Tüm paketler `net11.0 + EF 11.0.0-preview.6.26359.118` hedefle, JWT 8.23, Hangfire 1.8.x (NETSTANDARD 2.0+ olduğu için 11 ile uyumlu), OData 8.2.x, Asp.Versioning 8.x
2. **AuditEntry Performansı:** Her SaveChanges'ta her prop için ayrı satır → Tek Request 100 entity update → 1000 AuditEntry satırı. Çözüm: `BatchSize=1000` ayarla, Opsiyonel: `AuditSettings:Enabled=true` false kapatılabilir
3. **OData + Global Query Filter UYUMSUZLUĞU:** ODataOptions.ApplyTo IQueryable EF Global Filter Status!=Deleted'i ZATEN UYGULAR. Silinenleri görmek istiyorsan IgnoreQueryFilters() ekle (Restore endpointinde kullan)
4. **Hangfire + Distributed Transaction:** Email SenderJob içinde 2 kez SaveChanges → TransactionScope kullanma (Hata durumunda Job Failed kaydedilsin, Retry ile tekrar denensin)
5. **PasswordSettings + LockoutSettings IOptions ile Enjekte:** Hardcode değere SAKIN YAZMA, IOptions<PasswordSettings>.Value al
6. **RateLimiting + Idempotency Sırası:** ÖNCE RateLimit çalışır (isteği reddeder), SONRA Idempotency Filter → Yanlış Idempotency key 100 kez atılırsa Rate Limit 10. istekten sonra keser
7. **CORS Sırası:** UseCors UseRateLimiter'dan ÖNCE, UseHttpsRedirection'dan SONRA (CORS headerları OPTIONS response'da olmalı)
8. **Notification Hub + MessageSize:** 32KB üzeri mesaj için ayrı FileStorage kullan, Notification Data JSON URL payload gönder (client download eder)
9. **SecurityStamp Middleware:** Her istekte DB VURUR — Kullanıcı başına MemoryCache 30 saniyelik (userId:SecurityStamp) cache ekle, DB yükünü azalt
10. **LocalFileStorage + Production:** NEVER Production Local kullan, AzureBlob/MinIO/AmazonS3

---

## Validation (Kabul Kriterleri 26 Madde)

### Build / Static Analysis
1. `dotnet build NetCoreTemplate.slnx` → 0 Hata, ≤ 10 Uyarı
2. `GetDiagnostics` → 0 Diagnostic
3. `dotnet ef migrations list --no-connect` → InitialCreate + V2_FullFeatures_AllEntities listelensin (2 migration)

### Core Integration Test (dotnet run + Postman / curl 13 test)
4. **RBAC Test:** Login SuperAdmin → Create Role "Test" → Assign Permission "Users.Create" → AssignRole Test User1 → User1 Login → Users.Create isteği 200, Logout ile SecurityStamp regenerate → User1 eski token 401 (SecurityStamp Middleware çalışıyor)
5. **Rate Limit Test:** 11 kez Login POST 10 sn'de → 11. istek 429 Too Many Requests
6. **Login Denemesi:** 6 kez yanlış şifre → 6. Login 401 "Hesap kilitlendi 15dk" → Admin UnlockUserCommand → Tekrar login 200
7. **CORS Production:** Origin: https://evil.com → Response CORS header YOK (izin verilmedi). Origin: AllowedOrigins[0] → 200 + CORS header var
8. **Health:** https://localhost:7000/health/ready → 200 JSON entries: sql-db=Healthy, smtp=Degraded (localhost maildev çalışmazsa), storage=Healthy, hangfire=Healthy
9. **Şifre Gücü:** Register şifre "qwe123" → 400 "Password must contain uppercase, special char"; "Qwerty123!@#" → 201 Created
10. **SecurityStamp Test:** Kullanıcı şifre değiştir (ChangePassword) → Eski access_token ile /me → 401 "Security stamp mismatch"
11. **LogoutAllDevices:** Admin userId=X için LogoutAllDevices → X kullanıcısı 2. cihazında istek → 401
12. **Hangfire Email Kuyruk:** Register (WelcomeEmail) → /hangfire dashboard: Succeeded job listesinde "EmailSenderJob - SendWelcome" 1 kayıt. HTTP isteği 300ms döner (eskiden SMTP 5000ms beklerdi)
13. **File Storage Test:** UploadFile avatar.jpg (2MB) → 200 fileUrl → Browser'da fileUrl 200 görüntüler; DeleteFile → tekrar fileUrl isterse 404
14. **Audit Entry Detaylı:** UserProfile FirstName = "Ahmet" → "Mehmet" güncelle → AuditEntries/GetByEntityId endpointte 1 satır PropertyName=FirstName, OldValue=Ahmet, NewValue=Mehmet (QUERY edilebilir, $filter='Mehmet' çalışır)
15. **API Versioning:** /api/v1/auth/me → 200; /api/v2/test/dummy → 200 v2-specific response
16. **OData Test:** /api/v1/odata/Users?$filter=Email eq 'a@b.com'&$select=Id,UserName&$orderby=CreatedDate desc&$count=true → 200 JSON @odata.count, value[] sadece Id,UserName (sadece 2 kolon gelmeli - Network tab'da kontrol)
17. **OpenTelemetry:** Console Output ActivitySource: "AspNetCore Server HttpRequest In" + "Microsoft.EntityFrameworkCore Command ExecuteReader" görünmeli
18. **Soft Restore:** User sil → SoftDeleted olanları listele (GetDeletedEntities) → Restore et → Normal User query'de tekrar görünür; AuditEntry'de "EntityRestored" kaydı
19. **SignalR Bildirim:** Browser 2 bağlantı (UserA, UserB) → Admin SendNotification UserA: "Yeni sipariş" → UserA alert kutusu açılır, UserB hiçbir şey görmez; Notifications/GetMyNotifications UserA'da 1 kayıt
20. **Idempotency + Rate Limit Birlikte:** 11 kez POST Register AYNI Idempotency-Key → İlk 10 → Rate Limit kesmeden önce 1. istek 201, 2-10 istek Idempotency cached 201, 11. istek 429 (Rate Limit öncelikli)
21. **Hangfire Auth:** /hangfire User (SuperAdmin değil) → 403 Forbidden; SuperAdmin → Dashboard açılır, Job listesi görünür
22. **CreatedBy Otomatik:** User1 login → Create Profile → AppUserProfile Table CreatedByUserId = User1.Id (boş DEĞİL)
23. **Seed Data:** Uygulama ilk başlarken → 6 Rol + 50+ Permission + 1 SuperAdmin kullanıcısı DB'de var
24. **PagedResponse + OData Count:** SystemLogs OData $count=true → PagedResponse.TotalCount == @odata.count (eşleşmeli)
25. **Program.cs ≤ 25 satır:** Satır sayısı check
26. **Katman Ref Zinciri:** Domain 0 ProjectRef, App → Domain only, Infra → App only, Api → Infra only (kesin döngüsüz)

---

## Risks + Mitigation (9 Risk)
1. 🔴 **Risk: OData + ApiResponse Format Conflict** — OData varsayılan olarak `{ @odata.context, @odata.count, value[] }` döner. Kullanıcı STANDART ApiResponse istiyor. **Çözüm:** Endpoint handler içinde `EnableQueryAttribute.AllowQueryOptions = AllowedQueryOptions.All | AllowedQueryOptions.Format`; `queryOptions.ApplyTo(_queryable, settings);` sonra OData IQueryable sonucu al, `IQueryable.Count()` ile totalCount hesapla, SONRA `ApiResponse.Paged(items, page, size, totalCount)` ile SAR — @odata.context/count propertylerini SİL, sadece ApiResponse JSON dön.
2. 🔴 **Risk: Hangfire + Net11 Preview Compatibility** — Hangfire.SqlServer 1.8.x .NET 8.0 hedefliyor. Net11-preview'da UseSqlServerStorage başarısız olabilir. **Mitigation:** Eğer başarısız → Hangfire.MemoryStorage kullan (Development/orta ölçek). Net 11 RTM sonrası Hangfire 2.x ile geç.
3. 🟡 **Risk: SecurityStamp Middleware DB Load** — Her istekte kullanıcı için SELECT FROM AppUsers → 10K RPS ise SQL bağlantısı patlar. **Çözüm:** IMemoryCache `_cache.GetOrCreate($"user-security-stamp:{userId}", entry => entry.SlidingExpiration = TimeSpan.FromSeconds(30); return user.SecurityStamp;)` — 30 sn taze cache.
4. 🟡 **Risk: OData $expand Infinite Loop** — User.Include(UserRoles).Include(Role).Include(Permissions)... MaxExpansionDepth=5 olsa da 10 tablo join performans. **Çözüm:** OData ayarlarında MaxExpansionDepth=3, ODataModelBuilder'da EntitySet NavigationPropertyBinding sınırlaması, AutoMapper ProjectTo<T> öncelikli kullan.
5. 🟡 **Risk: LocalFileStorage + Path Traversal Attack** — Kullanıcı "../secrets.json" upload ederse. **Çözüm:** Path.GetFileName(path) SADECE dosya adı al, container isimlerini whitelist ile kontrol et, extension whitelist (.jpg, .png, .pdf vb.)
6. 🟡 **Risk: SignalR Backplane** — 2+ pod çalıştırırsan UserA pod1'de, UserB pod2'de; pod1'den Bildirim gönderirsen UserB POD2'deki Hub client ALMAZ (InMemory default SignalR). **Mitigation:** Redis Backplane ekle (Plan içinde yok, Risk — Not: Dokümanda not bırak, AddStackExchangeRedis()).
7. 🟢 **Risk: OpenTelemetry SDK Memory Leak** — 1.9.x Instrumentation.AspNetCore bazı sürümler leak. **Çözüm:** 1.10.x veya daha yeni kararlı sürüm kullan; Development'ta Console exporter, Production OTLP Collector.
8. 🟢 **Risk: AuditEntry Tablo Büyümesi (1 yılda 10M satır)** — Çok büyük hacim. **Çözüm:** NightlyCleanupJob içinde AuditEntry CreatedAt > 365 gün olanları arşiv tablosuna taşı veya hard delete; Index bakımları otomatik.
9. 🟢 **Risk: AppDbContext.SaveChanges Audit Loop (Infinite)** — `ProcessAudit()` AuditEntry ekler → Tekrar SaveChanges → Tekrar ProcessAudit. **Çözüm:** ProcessAudit içinde `if (type == typeof(AuditEntry)) continue;` ekle ve SaveChanges çağrılırken ProcessAudit bir kez çalışsın (flag: `bool _auditProcessed` constructor + SaveChanges içinde check + reset).

---

## Son Özet: 14 Özellik + 9 Aşama + 26 Doğrulama + 9 Risk
Bu plan onaylandığında:
- 400+ satır Domain/Entity/Enum/Interface
- 1500+ satır Application CQRS + Validator + Middleware
- 1200+ satır Infrastructure Implementation + Hangfire Job + Config
- 1200+ satır Api Middleware + Endpoint + OData + SignalR + Swagger/Scalar
- 1 Migration (V2_FullFeatures_AllEntities)
- 1 Seed Data
- 1 AI_RULES_FOR_THIS_REPO.md Güncelleme

Toplam ~4500+ satır kod değişikliği. Tahmini Build 0 Hata sonrası TEST komutları: `dotnet run --project Presentation/NetCoreTemplate.Api` + tarayıcı testleri (health, hangfire dashboard, swagger, scalar, odata).
