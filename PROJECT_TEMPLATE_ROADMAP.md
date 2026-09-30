# NetCoreTemplate - Proje Yol Haritası ve Geliştirme Rehberi

> Bu doküman NetCoreTemplate şablon projesi için mimari özeti, sonraki geliştirme adımlarını ve yeni entity/feature ekleme rehberini içerir. Sonraki projelerde bu şablon kullanılacağı zaman bu dokümandaki adımları izleyin.

---

## 1. Mimari Özeti

```
Presentation (Sunum Katmanı)
└── NetCoreTemplate.Api (Minimal API + IEndpoint Pattern, Program.cs 20 satırdan az)
    → Sorumlulukları: HTTP endpoint tanımlama, middleware, auth, DI, Swagger

Core (Çekirdek İş Katmanı)
├── NetCoreTemplate.Application (İş Kuralları, CQRS)
│   └── Features/[Biri] : IRequest, IRequestHandler, FluentValidation Validator'lar
│   └── DTOs: Giriş/çıkış modelleri
│   └── Behaviors: MediatR pipeline (Validation, Logging)
│   └── Mappings: AutoMapper Profile'lar
│   └── Exceptions: Özel exception sınıfları
│   └── DependencyInjection.cs: AddApplicationServices()
│   Kural: SADECE Domain katmanını REFERANS ALIR
│
├── NetCoreTemplate.Domain (Varlıklar + Kurallar)
│   └── Entities: AppUser, AppUserProfile, AppUserRefreshToken, AppUserActivited, Setting, SystemLog
│   └── Seedworks/BaseEntity: Id, Created/Modified/DeletedDate, Created/Modified/DeletedBy, Status (soft delete)
│   └── Enums: EntityStatus, UserActivityType, TokenType, TwoFactorType, SystemLogLevel
│   └── Interfaces: IAppDbContext, IUnitOfWork, IRepository<T> + HER ENTİTY İÇİN ÖZEL REPO INTERFACE
│   └── Interfaces/Security: IPasswordHasher, ITokenService, IUserActivityLogger
│   └── Interfaces/Services: IEmailService
│   Kural: HİÇBİR KATMANI REFERANS ALMAZ (EN ÜST)
│
└── NetCoreTemplate.Infrastructure (Altyapı)
    └── Persistence: AppDbContext, Configurations, Repositories, UnitOfWork
    └── Security: PasswordHasher (BCrypt), JwtTokenService, TokenGenerator, EncryptionHelper (AES), HashHelper, UserActivityLogger
    └── Services: EmailService (MailKit)
    └── Logging: SerilogConfigurator (Console + MSSqlServer sink (SystemLogs tablosu)
    └── DependencyInjection.cs: AddInfrastructureServices()
    Kural: SADECE Application katmanını REFERANS ALIR
```

### Katman Referans Zinciri
Api → Infrastructure → Application → Domain
❌ Döngüsel referans YOK, ❌ Domain hiç ProjectReference almaz

### Kullanılan Pattern'ler
- **CQRS (MediatR):** Her iş → Ayrı Command/Query + Handler + Validator
- **Repository + UnitOfWork:** Her entity için ÖZEL Repository (orn: IAppUserRepository) + Generic base (IRepository<T>) + IUnitOfWork
- **FluentValidation:** Her Command/Query için Validator sınıfı (null check, ValidationBehavior otomatik çalışır
- **AutoMapper:** Entity ↔ DTO mapping
- **Serilog:** Yapılandırılmış loglama + Console + SystemLogs (MSSQL)
- **SaveChanges Audit:** AppDbContext.ProcessAudit() otomatik Created/Modified/Deleted tarih stamp + soft delete + Değişiklikleri AppUserActivited tablosunda loglar

---

## 2. Mevcut Özellikler (Tamamlanan)
☑️ Kullanıcı Yönetimi: AppUser + AppUserProfile (CRUD + Profile
☑️ Authentication & Authorization: JWT Bearer, Access Token + Refresh Token (rotate'lı), 2FA (Email)
☑️ Auth Endpoint'leri: /api/auth/{login, register, refresh, forgot-password, reset-password, enable-2fa, verify-2fa, logout, me}
☑️ Şifreleme: BCrypt WorkFactor=11, AES-256 Enc/Dec, SHA256/512 + HMAC helper
☑️ Kullanıcı Aktivite Logu: AppUserActivited tablosu (Login, Logout, PasswordChanged, EntityCreated/Updated/Deleted vb.
☑️ Sistem Logu: SystemLog tablosu (Exception Handling Middleware + Serilog MSSqlServer Sink
☑️ Email Servisi: MailKit SMTP (Hoş geldin, şifre sıfırlama, email doğrulama, 2FA kodu)
☑️ Global Exception Handling: JSON cevap + SystemLog tablosuna kayıt
☑️ Ayarlar: Setting tablosu (Global + User bazlı Key/Value)
☑️ Swagger: JWT Authentication ile güvenlik şeması

---

## 3. Sonraki Geliştirme Adımları (Önerilen Öncelik Sırası)

### 🔴 Yüksek Öncelik (Kullanılmadan önce tamamlanmalı)
1. **EF Core Migration Oluşturma:**
   ```powershell
   cd Core/NetCoreTemplate.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ../../Presentation/NetCoreTemplate.Api
   dotnet ef database update --startup-project ../../Presentation/NetCoreTemplate.Api
   ```
2. **Production Güvenlik Ayarları:**
   - `JwtSettings.SecretKey` appsettings'ten silin, User Secrets veya Azure Key Vault kullan
   - Strong password policy uygulayın (şu an 8 karakter + büyük/küçük + sayı
   - CORS politikasını "AllowAll" yerine production'da gerçek domainleri listeleyin
   - HTTPS zorunlu olsun + HSTS header ekle
   - Rate Limiting ekle (Login/Register brute force koruması için)
3. **Rol ve Yetki Sistemi:** `AppRole`, `AppUserRole`, `AppPermission`, `AppRolePermission` entity'leri + [Authorize(Roles="Admin")]
4. **Email Provider Değiştir:** MailKit GPL lisanslı. Production için: SendGrid / Mailgun / FluentEmail (MIT lisanslı) geçin veya System.Net.Mail (eski)

### 🟡 Orta Öncelik
5. **TOTP 2FA:** Google Authenticator/Microsoft Authenticator desteği (Otp.NET paket
6. **Distributed Cache:** Microsoft.Extensions.Caching.StackExchangeRedis → IDistributedCache
7. **Background Jobs:** Hangfire veya Quartz.NET → Recurring Job'lar, email kuyruğu
8. **Health Check:** Microsoft.Extensions.Diagnostics.HealthChecks → /health endpoint'i (DB, SMTP, Redis)
9. **API Versioning:** Asp.Versioning.Http
10. **Rate Limiting:** Microsoft.AspNetCore.RateLimiting
11. **Localization:** Resources (.resx) + RequestLocalizationMiddleware (TR/EN)

### 🟢 Düşük Öncelik / Opsiyonel
12. **CQRS Read/Write Ayırma:** Write DB + Read DB (Dapper ile okuma)
13. **SignalR:** Gerçek zamanlı bildirimler
14. **Blob Storage:** Azure Blob / MinIO / AWS S3 (Avatar ve dosya yüklemeleri için)
15. **Audit History:** Tüm entity değişikliklerini ayrı bir AuditLogs tablosunda (zaten SaveChanges override kısmen yapıyor)
16. **Unit & Integration Test:** xUnit + TestContainers (Test DB)
17. **Docker Support:** Dockerfile + docker-compose.yml (API,
18. **API Gateway / Reverse Proxy: YARP / nginx
19. **OpenTelemetry:** Metrics + Tracing (Prometheus + Grafana)
20. **Multi Tenant:** Multi-tenant mimari (Client/Şirket/

---

## 4. Yeni Entity Ekleme Rehberi (7 ADIM)

Yeni bir entity (örn: `Product`, `Category` vb.) eklemek için bu 7 adımı sırayla izleyin. Örnek entity: `Product`

### Adım 1: Domain Katmanı
1. `Domain/Entities/Product.cs` oluştur
   - `BaseEntity`'den kalıtım al
   - Property setterları **private/protected** yap
   - **Constructor** oluştur ve non-nullable property'leri constructor'da set et
   - Domain metodları ekle (örn: `UpdatePrice()`, `Deactivate()`
2. Gerekliyse `Domain/Enums/` içinde yeni enum ekle `ProductStatus.cs`)
3. `Domain/Interfaces/Repositories/IProductRepository.cs` oluştur
   - `IRepository<Product>` implement et
   - Özel metodlar ekle (örn: `Task<Product?> GetBySkuAsync(string sku)`)
4. `Domain/Interfaces/IUnitOfWork.cs` içine: `IProductRepository Products { get; }` property'sini ekle
5. (Opsiyonel) Domain event fırlatacaksan Domain Event ekle

### Adım 2: Infrastructure Katmanı - EF Core
1. `Infrastructure/Persistence/Configurations/ProductConfiguration.cs` oluştur (IEntityTypeConfiguration<Product>)
   - PK, FK, Unique Index, string MaxLength, ilişkileri tanımla
2. `Infrastructure/Persistence/Repositories/ProductRepository.cs` oluştur
   - `Repository<Product>, IProductRepository` implement
   - Özel metodları yaz
3. `Infrastructure/Persistence/AppDbContext.cs` → `DbSet<Product> Products => Set<Product>();` EKLE
4. `Infrastructure/Persistence/UnitOfWork.cs` → Lazy init property EKLE:
   ```csharp
   private IProductRepository? _products;
   public IProductRepository Products => _products ??= new ProductRepository(_context);
   ```
5. `Infrastructure/DependencyInjection.cs` → servis kaydını EKLE:
   ```csharp
   services.AddScoped<IProductRepository, ProductRepository>();
   ```

### Adım 3: Application Katmanı - DTO'lar
1. `Application/DTOs/Products/ProductDto.cs` (Read DTO record)
2. `Application/DTOs/Products/CreateProductRequestDto.cs` (Create Input)
3. `Application/DTOs/Products/UpdateProductRequestDto.cs` (Update Input)
4. Gerekirse List/Filter/Paginated DTO'ları ekle

### Adım 4: Application Katmanı - Validator'lar
1. Her Command/Query için FluentValidation Validator yaz
   - `CreateProductCommandValidator.cs`, `UpdateProductCommandValidator.cs`, `GetProductQueryValidator.cs`

### Adım 5: Application Katmanı - Features (CQRS Command/Query/Handler)
1. Features/Products/Commands/Create/
   - CreateProductCommand.cs → IRequest<ProductDto>
   - CreateProductCommandHandler.cs → IRequestHandler (IUnitOfWork.Products.AddAsync → SaveChanges → Map → return)
   - CreateProductCommandValidator.cs
2. Features/Products/Commands/Update/
   - UpdateProductCommand + Handler + Validator
3. Features/Products/Commands/Delete/
   - DeleteProductCommand + Handler
4. Features/Products/Queries/GetById/
   - GetProductByIdQuery + Handler
5. Features/Products/Queries/GetAll/
   - GetAllProductsQuery (filtre, sayfalama) + Handler
6. `Application/Mappings/MappingProfile.cs` → Product ↔ Product DTO mapping EKLE

### Adım 6: Presentation (API) Katmanı - Endpoints
1. `Api/Endpoints/ProductsEndpoints.cs` oluştur, `IEndpoint` implement et
2. `Map()` metodu içinde:
   ```csharp
   var group = app.MapGroup("api/products").WithTags("Products").WithOpenApi().RequireAuthorization();
   group.MapGet("{id}", async ...);
   group.MapPost("", async ...);
   group.MapPut("{id}", async ...);
   group.MapDelete("{id}", async ...);
   ```
3. (Otomasyon: MapEndpoints() reflection ile otomatik olarak ekler, ekstra bir şey yapmana gerek yok

### Adım 7: Test + Dokümantasyon
1. Migration eklemeden önce **Build + dotnet build -warnaserror 0 hata 0 uyarı olduğunu kontrol et
2. `AI_RULES_FOR_THIS_REPO.md` dokümanını GÜNCELLE (Yeni entity örnekleri dahil)
3. Swagger UI'dan test et
4. Veritabanında AppUserActivited tablosunda EntityCreated/Updated/Deleted loglarının otomatik düştüğünü doğrula
5. Exception fırlat → SystemLog tablosuna kayıt olduğunu kontrol et

---

## 5. Sık Kullanılan Komutlar
```powershell
# Solution restore + build
dotnet restore NetCoreTemplate.slnx
dotnet build NetCoreTemplate.slnx -warnaserror

# Migration
dotnet ef migrations add InitialCreate -p Core/NetCoreTemplate.Infrastructure -s Presentation/NetCoreTemplate.Api
dotnet ef database update -p Core/NetCoreTemplate.Infrastructure -s Presentation/NetCoreTemplate.Api

# Run API
cd Presentation/NetCoreTemplate.Api
dotnet run
# Swagger: https://localhost:7000/swagger

#User Secrets (Production gizli anahtarlar için)
dotnet user-secrets init --project Presentation/NetCoreTemplate.Api
dotnet user-secrets set "JwtSettings:SecretKey" "COK_GIZLI_BIR_ANAHTAR_123"
```

---

## 6. Yaygın Hatalar ve Çözümleri
| Hata | Çözüm |
|------|-------|
| `Nullable disable warning | Entity constructor'ında non-nullable property'i set et veya `= null!` kullanma (tercih) constructor |
| `IUnitOfWork repository property'ı null dönüyor | UnitOfWork.cs'de lazy init'i unutma |
|SaveChanges infinite loop | ProcessAudit'te AppUserActivited ve SystemLog entity'lerini ATLAMALISIN (skip et) |
|JWT doğrulama hatası | JwtSettings'teki Issuer, Audience ve SecretKey appsettings ile JwtBearer options aynı olsun |
|Login başarısız ama hata yok | AppUserActivited tablosunu kontrol et. LoginFailed logu var mı? LockoutEndDate dolmuş olabilir mi? |
|Swagger Authorize butonu yok | DependencyInjection.cs SwaggerGen AddSecurityDefinition ve AddSecurityRequirement çağrılarını kontrol et |
|Email gönderimi başarısız | SmtpHost/SmtpPort ve EnableSsl ayarlarını kontrol et, localhost:25 yerine mailtrap.io gibi fake smtp kullan |
