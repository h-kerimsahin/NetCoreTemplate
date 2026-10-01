# 🏗️ NetCoreTemplate — Production Grade .NET 10 Clean Architecture API

> **Production hazır**, çok katmanlı, güvenlik odaklı modern ASP.NET Core Web API template'i.  
> Identity+RBAC, MediatR (CQRS), Hangfire, SignalR, OData v4, OpenTelemetry, 3-katman Rate Limit, Idempotency, Soft-Delete + Audit, Local/Azure File Storage, Swagger + Scalar UI.

---

## 🛡️ Tech Stack & Badges

| Katman | Teknoloji |
|--------|-----------|
| **Runtime** | ![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet) ASP.NET Core Web API (Minimal API) |
| **ORM** | ![EF Core](https://img.shields.io/badge/EF%20Core-8+-512BD4?logo=microsoftsqlserver) SQL Server / LocalDB |
| **Pattern** | **Clean Architecture 4 Katman**, **CQRS** (MediatR 14 Single Package) |
| **Auth** | JWT (Access 15dk / Refresh 7g), ASP.NET Identity Custom Model, **2FA**, **RBAC** (Role+Permission) |
| **Jobs** | Hangfire Dashboard + Recurring Jobs (Memory Storage Fallback) |
| **Realtime** | SignalR Notification Hub |
| **Query** | OData v4 (`$filter`, `$select`, `$orderby`, `$metadata`) |
| **Observability** | OpenTelemetry 1.19.1 (Trace/Metric/OTLP) + Prometheus `/metrics` |
| **Logging** | Serilog Structured Log (Console/File/MSSQL sinks) |
| **Docs** | Swagger UI + Scalar UI (v1 & v2 versioned OpenAPI) |
| **Security** | 3-Katman Rate Limit, X-Idempotency-Key Filter, Path Traversal Koruması, CORS Secure-by-Default, Production Seed Off |
| **Validation** | FluentValidation + Mapster Mapping |

---

## 📁 Proje Yapısı (Clean Architecture — 4 Katman)

```
NetCoreTemplate/
├── Core/
│   ├── NetCoreTemplate.Domain/            # Entity, Enum, IRepository, Value Object
│   ├── NetCoreTemplate.Application/       # DTO, CQRS Command/Query, Validator, Mappings
│   └── NetCoreTemplate.Infrastructure/    # EF Core, UoW, Hangfire, Serilog, Storage, Email, OTel
└── Presentation/
    └── NetCoreTemplate.Api/               # Minimal API Endpoints (11 adet), SignalR Hub, UI, Program
```

| Katman | Örnek İçerik |
|---|---|
| **Domain** | `AppUser`, `AppRole`, `AppPermission`, `EntityStatus`, `IRepository<T>` |
| **Application** | `LoginCommand`, `RegisterCommand`, `MeQuery`, `FluentValidation Validator` |
| **Infrastructure** | `AppDbContext`, `UnitOfWork`, `LocalFileStorageService`, `HangfireJobScheduler`, `EmailSender` |
| **API** | `AuthEndpoints.cs`, `RolesEndpoints.cs`, `NotificationHub.cs`, `Program.cs` |

---

## 👤 Default Seed (SADECE Development Ortamı)

> ⚠️ **Production'da bu kullanıcı OLUŞTURULMAZ** — AppDbInitializer `IHostEnvironment.IsDevelopment()` ile kontrol eder.

| Bilgi | Değer |
|---|---|
| Kullanıcı Adı | `SuperAdmin` |
| E-posta | `superadmin@netcoretemplate.com` |
| Şifre | `Qwerty123!` |
| Rol | `SuperAdmin` |
| Otomatik Seed Roller | `SuperAdmin`, `Admin`, `User` |
| Permissions | Uygun permissionların tümü tabloya seed edilir |

> 📌 **Production için**: İlk SuperAdmin'i DB script veya CLI ile manuel oluştur.

---

## 🚀 Quick Start (5 Adımda Çalıştır)

```bash
# 1. Restore
dotnet restore

# 2. DB'yi oluştur (Migration)
cd Presentation/NetCoreTemplate.Api
dotnet ef database update

# 3. Çalıştır
dotnet run
```

| Adres | Ne |
|---|---|
| 🌐 HTTP | `http://localhost:5084` |
| 🔐 HTTPS | `https://localhost:7084` |
| 📘 **Swagger UI** | `http://localhost:5084/swagger` |
| 🎨 **Scalar UI (Modern)** | `http://localhost:5084/scalar` & `/scalar/v2` |
| ⏱️ Hangfire Dashboard | `http://localhost:5084/hangfire` (SuperAdmin) |
| 🏥 Health Readiness | `http://localhost:5084/api/v1/health/ready` |
| 📊 Prometheus Metrics | `http://localhost:5084/metrics` |
| 🧩 OData $metadata | `/api/v1/odata/$metadata` & `/api/v2/odata/$metadata` |

---

## 🔌 Tüm Endpointler (35+ REST + 8 UI/Dashboard)

Tüm API'ler **standart zarflı response** (`ApiResponse<T>`) döner:

```json
{
  "statusCode": 200,
  "message": "İşlem başarılı.",
  "data": { "...": "..." },
  "errors": null,
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 123
}
```

> 📌 Sayfalı endpointler: `?page=1&size=20` (varsayılan)  
> 📌 Idempotency: POST/PUT'da `X-Idempotency-Key: <guid>` header → aynı işlemi 2. kez göndermezsiniz.

---

### 🔐 GRUP A — Authentication (12 Endpoint)
**Prefix:** `api/v1/auth`

| # | Method | Route | Auth | Idem | Açıklama |
|---|---|---|---|---|---|
| A1 | `POST` | `/login` | 🔓 AllowAnonymous | ✅ | Kullanıcı girişi (Email/Username + Password) |
| A2 | `POST` | `/register` | 🔓 AllowAnonymous | ✅ | Yeni kullanıcı kaydı |
| A3 | `POST` | `/refresh` | 🔓 AllowAnonymous | ✅ | Access Token yenile (Refresh Token) |
| A4 | `POST` | `/change-password` | 🔑 Bearer | ✅ | Giriş yapan kullanıcı kendi şifresini değiştir |
| A5 | `POST` | `/confirm-email` | 🔓 AllowAnonymous | ✅ | E-posta doğrula |
| A6 | `POST` | `/forgot-password` | 🔓 AllowAnonymous | ✅ | Şifre sıfırlama token talebi |
| A7 | `POST` | `/reset-password` | 🔓 AllowAnonymous | ✅ | Token ile yeni şifre ayarla |
| A8 | `POST` | `/enable-2fa` | 🔑 Bearer | ✅ | 2FA Etkinleştir |
| A9 | `POST` | `/verify-2fa` | 🔓 AllowAnonymous | ✅ | 2FA Kod Doğrula / Giriş |
| A10 | `POST` | `/logout` | 🔑 Bearer | ✅ | Tek cihazdan çıkış (Refresh Token Revoke) |
| A11 | `POST` | `/logout-all-devices` | 🔑 Bearer | ✅ | **Tüm** cihazlardan çıkış (SecurityStamp Yenile) |
| A12 | `GET` | `/me` | 🔑 Bearer | ❌ | Mevcut kullanıcı bilgisi + Roller + Permissions |

#### Örnek — Login Request (A1)
```json
{
  "emailOrUserName": "SuperAdmin",
  "password": "Qwerty123!",
  "rememberMe": true
}
```

#### Örnek — Login Response (200 OK)
```json
{
  "statusCode": 200,
  "message": "Giriş başarılı.",
  "data": {
    "accessToken": "<JWT ACCESS TOKEN — 15dk>",
    "accessTokenExpiresAt": "2026-10-01T11:30:00Z",
    "refreshToken": "<REFRESH TOKEN — 7 gün>",
    "refreshTokenExpiresAt": "2026-10-08T11:15:00Z",
    "user": {
      "id": "00000000-0000-0000-0000-000000000001",
      "userName": "SuperAdmin",
      "email": "superadmin@netcoretemplate.com",
      "roles": ["SuperAdmin"],
      "permissions": ["*.*"]
    }
  }
}
```

---

### 🛠️ GRUP B — User Management (5 Endpoint)
**Prefix:** `api/v1/users-management`  
**Gerekli Rol:** `Admin` **VEYA** `SuperAdmin` (Tüm grup)

| # | Method | Route | Idem | Açıklama |
|---|---|---|---|---|
| B1 | `POST` | `/assign-role` | ✅ | Kullanıcıya **yeni roller ata** (eski roller silinir, üzerine yazar) |
| B2 | `POST` | `/revoke-role` | ✅ | Kullanıcıdan **belirli rolleri geri al** |
| B3 | `POST` | `/{userId}/logout-all-devices` | ✅ | Admin → *Herhangi bir kullanıcıyı* tüm cihazlardan at |
| B4 | `POST` | `/{userId}/unlock` | ✅ | Kullanıcı kilidini aç (5 başarısız girişte kilit) |
| B5 | `GET` | `/paged?page=1&size=20` | ❌ | Tüm kullanıcıları sayfalı listele |

#### Örnek — AssignRole (B1) Request
```json
{
  "userId": "a1b2c3d4-...",
  "roleIds": [ "role1-guid", "role2-guid" ]
}
```

---

### 🏷️ GRUP C — Roles & Permissions (6 Endpoint)
**Prefix:** `api/v1/roles`  
**Gerekli Rol:** `Admin` **VEYA** `SuperAdmin`

| # | Method | Route | Idem | Açıklama |
|---|---|---|---|---|
| C1 | `POST` | `/` | ✅ | Yeni rol oluştur (soft-delete tabanlı) |
| C2 | `PUT` | `/{id:guid}` | ✅ | Rol adını & açıklamasını güncelle |
| C3 | `DELETE` | `/{id:guid}` | ❌ | Rol sil (**Soft-Delete**, Status=Deleted) |
| C4 | `POST` | `/{roleId}/permissions` | ✅ | Role **izinler ata** (mevcutları sil, yeni listeyi yazar) |
| C5 | `GET` | `/{id:guid}` | ❌ | Rol + İzinler detay getir |
| C6 | `GET` | `/?page=1&size=20` | ❌ | Tüm rolleri sayfalı listele (OData destekli) |

#### Örnek — AssignPermission (C4)
```json
{
  "roleId": "<ROL GUID>",
  "permissionIds": [ "perm-view-users", "perm-manage-products" ]
}
```

---

### 👤 GRUP D — User Profiles (3 Endpoint)
**Prefix:** `api/v1/user-profiles`  
**Auth:** 🔑 Bearer JWT (Login olan herkes)

| # | Method | Route | Idem | Açıklama |
|---|---|---|---|---|
| D1 | `GET` | `/{id:guid}` | ❌ | ID'ye göre kullanıcı profil bilgisi |
| D2 | `GET` | `/?page=1&size=20` | ❌ | Tüm profilleri sayfalı listele |
| D3 | `PUT` | `/mine` | ✅ | **KENDİ** profili güncelle (JWT'den userId çekilir) |

#### Örnek — UpdateMyProfile (D3)
```json
{
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "birthDate": "1992-03-15T00:00:00Z",
  "phoneNumber": "+905329876543",
  "address": "Cadde 1",
  "city": "Ankara",
  "country": "Turkey",
  "avatarUrl": "/uploads/avatars/2026/10/01/abc123.png",
  "bio": "Yazılım Geliştirici"
}
```

---

### 💾 GRUP E — File Storage / Upload (2 Endpoint)
**Prefix:** `api/v1/file-storage`  
**Auth:** 🔑 Bearer JWT

> 🚧 **İzin Verilen Container (Whitelist — aksi 400):** `avatars` · `documents` · `temp` · `exports`

| # | Method | Route | Idem | Açıklama |
|---|---|---|---|---|
| E1 | `POST` | `/upload` | ✅ | Multipart form dosya yükle (JWT + Antiforgery kapalı) |
| E2 | `DELETE` | `/delete?filePath=/uploads/.../x.png` | ❌ | Relative URL ile dosya sil (Path Traversal Korumalı) |

#### Örnek — Upload (E1)
**Content-Type:** `multipart/form-data`

| Form Field | Tip | Açıklama |
|---|---|---|
| `file` | IFormFile | **Zorunlu**, yüklenecek dosya |
| `container` | string | `avatars` / `documents` / `temp` / `exports` |

**Upload Response (201 Created):**
```json
{
  "statusCode": 201,
  "message": "Dosya yüklendi.",
  "data": {
    "url": "/uploads/avatars/2026/10/01/a1b2c3d4.png",
    "fileName": "<guid>_profil.png",
    "originalName": "profil.png",
    "size": 2048576
  }
}
```

> 🛡️ **Güvenlik:** `DeleteFileAsync` + `FileExistsAsync` → `Path.GetFullPath()` normalize + `StartsWith(WebRootPath)` kontrol → **Path Traversal saldırılarına karşı %100 koruma**.

---

### 🔔 GRUP F — Notifications + SignalR Hub (4+1)
**Prefix:** `api/v1/notifications` + **Hub:** `/hubs/notification`

| # | Method | Route | Rol | Idem | Açıklama |
|---|---|---|---|---|---|
| F1 | `POST` | `/send` | `Admin\|SuperAdmin` | ✅ | Bildirim gönder (DB + **SignalR Realtime Push**) |
| F2 | `GET` | `/mine?page=1&size=20` | 🔑 Herkes | ❌ | Benim bildirimlerim (Tarih DESC) |
| F3 | `POST` | `/{id}/mark-as-read` | 🔑 Herkes | ✅ | Tek bildirim okundu |
| F4 | `POST` | `/mark-all-as-read` | 🔑 Herkes | ✅ | **Tüm** bildirimlerim okundu |

#### Örnek — Bildirim Gönder (F1)
```json
{
  "userId": "<KULLANICI GUID veya NULL (Herkese)>",
  "title": "Yeni Sipariş",
  "message": "#1234 nolu siparişiniz kargoda.",
  "type": 1,
  "data": "{\"orderId\": 1234,\"link\":\"/orders/1234\"}"
}
```
> `type`: `0=Info, 1=Success, 2=Warning, 3=Error`

#### 🛰️ SignalR Hub (JS Client Örnek)
```javascript
const hub = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7084/hubs/notification?access_token=" + JWT_TOKEN)
  .build();

hub.on("ReceiveNotification", (notif) => console.log("Bildirim!", notif));
await hub.start(); // Baglanti kurulunca user kendi grubuna otomatik eklenir
```

---

### 🧾 GRUP G — Audit Entries / Değişiklik Gecmişi (2)
**Prefix:** `api/v1/audit-entries`  
**Rol:** `Admin \| SuperAdmin`

| # | Method | Route | Açıklama |
|---|---|---|---|
| G1 | `GET` | `/entity/{entityName}/{entityId}?page=1&size=20` | **Tek entity** değişiklik geçmişi (örn. `AppUser`, `AppRole`) |
| G2 | `GET` | `/user/{changedByUserId}?page=1&size=20` | **Bir kullanıcının** yaptığı TÜM değişiklikler |

Her kayıt: `ChangeType = Added / Modified / Deleted / Restored` + `OldValues` + `NewValues` (JSON diff).

---

### 🔄 GRUP H — Soft Delete Geri Yükleme (2)
**Prefix:** `api/v1/soft-restore`  
**Rol:** `Admin \| SuperAdmin`

| # | Method | Route | Idem | Açıklama |
|---|---|---|---|---|
| H1 | `GET` | `/deleted/{entityTypeName}?page=1&size=20` | ❌ | Silinmiş kayıtları listele (`appuser\|user`, `approle\|role`) |
| H2 | `POST` | `/restore` | ✅ | Silinmiş kaydı geri yükle (`Status: Deleted → Active`) |

```json
// Restore (H2) Request
{
  "entityTypeName": "appuser",
  "entityId": "<silinmis kaydin GUID>"
}
```

---

### 📝 GRUP I — System Logs & User Activities (2)
**Prefix:** `api/v1/system-logs` · **Auth:** 🔑 Bearer JWT

| # | Method | Route | Açıklama |
|---|---|---|---|
| I1 | `GET` | `/?level=Error&page=1&size=20` | Sistem logları (Serilog → DB). `level`: Information/Warning/Error/Fatal |
| I2 | `GET` | `/activities?type=Login&page=1&size=20` | Kullanıcı aktiviteleri. `type`: Login/Logout/FailedLogin/PasswordChange/ProfileUpdate/RoleAssigned... |

---

### 🏥 GRUP J — Health / Monitoring (4+2)
**Prefix:** `api/v1/health` · **Auth:** 🔓 AllowAnonymous

| # | Method | Route (Endpoint) | Açıklama | Cloud MW Alternatif |
|---|---|---|---|---|
| J1 | `GET` | `/live` | Liveness — sadece HTTP pipeline çalışıyor mu? | `/health/live` (MapHealthChecks) |
| J2 | `GET` | `/ready` | Readiness — **DB + SMTP + Hangfire** bağlantısı denetle | `/health/ready` (Custom JSON Writer) |
| J3 | `GET` | `/detailed` | Detaylı — Health kayıtlarının tümü + Exception/Data | - |
| J4 | `GET` | `/metrics` | Bilgi: Prometheus endpoint `/metrics` | - |

#### Prometheus Gerçek Scraping Endpoint
- **URL:** `/metrics`
- **Format:** text/plain (Prometheus Exposition)
- **Örnek Metrikler:** `process_cpu_seconds_total`, `dotnet_total_memory_bytes`, `http_request_duration_seconds`

---

### 🧪 GRUP K — V2 API Test (1 Endpoint)
**Prefix:** `api/v2/test`

| Method | Route | Açıklama |
|---|---|---|
| `GET` | `/dummy` | API Versioning v2 çalışma testi (Returns v2.0 JSON) |

---

### 🎨 GRUP L — UI Dashboards
**Sadece `env.IsDevelopment()` → production'da kapalıdır (Hangfire hariç).**

| URL | Ne | Erişim |
|---|---|---|
| `/swagger` | Swagger UI v1 + v2 (OpenAPI) | Development (Auth için Authorize butonu + Bearer) |
| `/swagger/v1/swagger.json` | v1 OpenAPI JSON | Development |
| `/swagger/v2/swagger.json` | v2 OpenAPI JSON | Development |
| `/scalar` | 🎨 Scalar UI — Modern görünen API dökümanı (v1) | Development |
| `/scalar/v2` | Scalar UI (v2) | Development |
| **`/hangfire`** | ⏱️ Hangfire Dashboard: Kuyruk, Job, Retries, Recurring (nightly-cleanup 02:00 UTC) | 🔐 **SuperAdmin Rolü** (Tüm ortamlar, custom auth filter) |
| `/api/v1/odata/$metadata` | OData v4 EDM Model v1 | AllowAnonymous |
| `/api/v2/odata/$metadata` | OData v4 EDM Model v2 | AllowAnonymous |

---

## 🛡️ Güvenlik Katmanları (Middleware & Filter)

| Katman | Nasıl Çalışır |
|---|---|
| **SecurityStampMiddleware** | Her istekte `JWT SecurityStamp` ↔ `DB SecurityStamp` eşleşmesi; eşleşmezse JWT geçersiz (LogoutAllDevices için) |
| **IdempotencyEndpointFilter** | POST/PUT'da `X-Idempotency-Key` → aynı key tekrar gelirse DB'den önbellek sonucu döner |
| **Rate Limit 3 Katman** | `AuthFixedWindow` (10/10dk), `GlobalSliding` (100/1dk), `IPConcurrency` (200/30dk) |
| **CORS Secure-by-Default** | `AllowCredentials = false` + `AllowedOrigins` boş → Admin/Frontend için doldurulur |
| **Upload Path Traversal** | `Path.GetFullPath` normalize + `StartsWith(WebRootPath)` → kök dışı dosyalar engellenir |
| **Production Seed Off** | SuperAdmin seed **sadece Development'ta** çalışır → Production'da bilinen şifre yok |
| **Upload Whitelist** | `avatars / documents / temp / exports` dışı container → 400 |

---

## 🏭 Production Deployment Checklist (ZORUNLU)

1. **[appsettings.Production.json](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/Presentation/NetCoreTemplate.Api/appsettings.Production.json)** → TÜM `REPLACE_WITH_*` alanlarını doldur:
   - `ConnectionStrings:DefaultConnection` (Azure SQL / MSSQL Production)
   - `AllowedHosts` (wildcard `*` KALDIRILDI, domain gir)
   - `CorsSettings.AllowedOrigins` (Frontend domain)
   - `EmailSettings` (Production SMTP)
   - `FileStorage:Provider = "Azure"` + Azure Blob Key
   - `Hangfire:WorkerCount` (CPU çekirdeği sayısı)

2. **JWT Secret — ASLA HARDCODE YAPMA**
   ```bash
   # Development (User Secrets)
   cd Presentation/NetCoreTemplate.Api
   dotnet user-secrets init
   dotnet user-secrets set "JwtSettings:SecretKey" "<MIN 64 KARAKTER RASTGELE>"
   ```
   **Production** → Azure Key Vault / AWS Secrets Manager / Docker Secrets / ENV

3. **İlk SuperAdmin** — Production'da Seed çalışmaz → DB Script / CLI ile manuel oluştur.

4. **Serilog Production Sink** → MSSQL / Seq / Elasticsearch ekle.

5. **K8s Probs Ayarları:**
   ```yaml
   livenessProbe:  { httpGet: { path: /health/live,   port: 5084 } }
   readinessProbe: { httpGet: { path: /health/ready,  port: 5084 } }
   ```

---

## 📌 Toplam Özet

| Kategori | Adet |
|---|---|
| 🔐 Auth Endpoint | **12** |
| 🛠️ User Management | **5** |
| 🏷️ Roles + Permissions | **6** |
| 👤 User Profiles | **3** |
| 💾 File Storage | **2** |
| 🔔 Notifications + SignalR Hub | **4 + 1** |
| 🧾 Audit Entries | **2** |
| 🔄 Soft Restore | **2** |
| 📝 System Logs + Activities | **2** |
| 🏥 Health + Monitoring | **4 + 2 Middleware Paths** |
| 🧪 V2 Dummy | **1** |
| 🎨 UI Dashboard (Swagger/Scalar/Hangfire/OData/Metrics) | **8** |
| **TOPLAM** | **43+ Endpoint** |

---

## 📄 Lisans & Dokümanlar

- Kurallar: [AI_RULES_FOR_THIS_REPO.md](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/AI_RULES_FOR_THIS_REPO.md)
- Özellik Planı (v2): [netcore_template_v2_features_plan.md](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/.trae/documents/netcore_template_v2_features_plan.md)
- Solution: [NetCoreTemplate.slnx](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/NetCoreTemplate.slnx)
