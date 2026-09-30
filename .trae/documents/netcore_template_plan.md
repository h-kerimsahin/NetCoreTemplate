# NetCoreTemplate Şablon Yapısı İyileştirme Planı (Güncellenmiş)

## Repository Research

### Mevcut Proje Yapısı
- **Katmanlı Mimari**: Core (Domain, Application, Infrastructure) + Presentation (Api) şeklinde temiz bir ayrım var
- **.NET 11.0**: Tüm projeler .NET 11.0 hedefliyor
- **OpenApi**: Api projesinde temel OpenApi/Swagger desteği var

### Mevcut Entity Durumları
- [BaseEntity.cs](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/Core/NetCoreTemplate.Domain/Entities/Seedworks/BaseEntity.cs): Tamamlanmış, auditable ve soft delete desteği var
- [AppUser.cs](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/Core/NetCoreTemplate.Domain/Entities/AppUser.cs): Kısmi dolu, constructor ve domain metodları eksik
- [AppUserProfile.cs](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/Core/NetCoreTemplate.Domain/Entities/AppUserProfile.cs): Kısmi dolu, ilişkiler tanımlı ama metodlar eksik
- [AppUserRefreshToken.cs](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/Core/NetCoreTemplate.Domain/Entities/AppUserRefreshToken.cs): **BOŞ** - Property'ler hiç tanımlanmamış
- [Setting.cs](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/Core/NetCoreTemplate.Domain/Entities/Setting.cs): **BOŞ** - Property'ler hiç tanımlanmamış
- [AppUserActivited.cs](file:///c:/GitRepo/h-kerimsahin/NetCoreTemplate/Core/NetCoreTemplate.Domain/Entities/AppUserActivited.cs): **BOŞ** - Artık KULLANICI AKTİVİTE LOGLARI için kullanılacak (Login, şifre değiştirme, profil güncelleme, entity CRUD)

### Yeni Eklenen Gereksinimler (Kullanıcı İsteği)
1. **Repository + UnitOfWork**: Her entity için ÖZEL repository (örn: IAppUserRepository, ISettingRepository) + generic base
2. **SaveChangesAsync Override**: DbContext'de SaveChanges/SaveChangesAsync ezilecek, entity değişikliklerinden otomatik AppUserActivited logu oluşturulacak
3. **Serilog**: ILogger<>'a enjekte edilecek, hem structured console log hem de veritabanına (SystemLog tablosuna + Serilog MSSQL sink) loglama
4. **AppUserActivited = User Activity Log**: Kullanıcı bazlı CRUD işlemleri, login, şifre sıfırlama, 2FA vb. aktiviteler burada loglanacak
5. **SystemLog = Yeni Entity**: Sistem hataları, EF hataları, middleware exception'ları, background job logları burada tutulacak
6. **Yol Haritası + AI Kural Dosyası**: Proje kökünde MD formatında template kuralları + yapay zeka yönergeleri dosyası oluşturulacak

---

## Files and Modules

### A. Domain Katmanı
1. Enum'lar: TwoFactorType.cs, TokenType.cs, UserActivityType.cs, SystemLogLevel.cs
2. Entity'ler: AppUserRefreshToken.cs, Setting.cs, AppUserActivited.cs (activity log), SystemLog.cs (yeni), AppUser.cs metodlar, AppUserProfile.cs metodlar
3. Interface'ler: IAppDbContext.cs, IUnitOfWork.cs, IRepository.cs (generic base), IAppUserRepository.cs, IAppUserProfileRepository.cs, IAppUserRefreshTokenRepository.cs, IAppUserActivitedRepository.cs, ISettingRepository.cs, ISystemLogRepository.cs, IPasswordHasher.cs, ITokenService.cs, IUserActivityLogger.cs, IEmailService.cs

### B. Application Katmanı
1. DependencyInjection.cs, Behaviors (ValidationBehavior, LoggingBehavior)
2. DTO'lar: Auth (15 dosya), UserProfile (3 dosya), SystemLog (2 dosya)
3. Features: Auth Commands (24 dosya - Command/Handler/Validator), Auth Queries (Me - 2 dosya), UserProfile Commands+Queries (5 dosya)
4. Exceptions: NotFound, Validation, Business, Unauthorized
5. MappingProfile.cs (AutoMapper)

### C. Infrastructure Katmanı
1. DependencyInjection.cs
2. Persistence: AppDbContext.cs (SaveChanges override), Configurations (6 entity), Repositories (Generic base + 6 özel repo), UnitOfWork.cs
3. Security: PasswordHasher.cs, JwtTokenService.cs, TokenGenerator.cs, EncryptionHelper.cs, HashHelper.cs, JwtSettings.cs, UserActivityLogger.cs
4. Services: EmailService.cs, EmailSettings.cs
5. Logging: SerilogConfigurator.cs (Console + MSSqlServer sink)

### D. Presentation - API Katmanı
1. DependencyInjection.cs (Auth, Swagger, CORS, Serilog)
2. Endpoints: IEndpoint.cs, EndpointExtensions.cs, AuthEndpoints.cs, UserProfileEndpoints.cs, SystemLogsEndpoints.cs
3. Middlewares: ExceptionHandlingMiddleware.cs (SystemLog yazar), RequestLoggingMiddleware.cs
4. Program.cs (TEMİZ - max 20 satır)
5. appsettings.json (ConnectionStrings, JwtSettings, EmailSettings, Serilog)

### E. Kök Dökümanlar
1. PROJECT_TEMPLATE_ROADMAP.md (Yol haritası + yeni entity ekleme rehberi)
2. AI_RULES_FOR_THIS_REPO.md (Yapay zeka için katman/entity/repo/CQRS kuralları + anti-pattern'ler)

---

## Implementation Steps (Kesin Sıra)

### Adım 0: csproj Proje Referansları + NuGet Paketleri
- Domain: Temel paketler
- Application: Domain ref + MediatR, FluentValidation, AutoMapper, DI Abstractions
- Infrastructure: Application ref + EF Core, EF Core SqlServer, Design, BCrypt, JWT, MailKit, Serilog + Serilog.Sinks.MSSqlServer + Enricher'lar
- Api: Infrastructure ref + JwtBearer, Serilog.AspNetCore

### Adım 1: Domain Katmanı
- Yeni Enum'lar: UserActivityType (Login, Logout, Register, PasswordChanged, ProfileUpdated, EntityCreated/Updated/Deleted vb.), SystemLogLevel (Information, Warning, Error, Fatal), TwoFactorType, TokenType
- AppUserActivited => User Activity Log: AppUserId, UserActivityType, Description (değişiklik özeti), IpAddress, UserAgent, EntityName, EntityId
- SystemLog (Yeni): LogLevel, Message, ExceptionType, StackTrace, Source, RequestPath, RequestMethod, UserId, IpAddress
- AppUserRefreshToken: Token (hashed), AppUserId, ExpiresAt, IsRevoked, RevokedAt, TokenType, IpAddress
- Setting: Key, Value, Description, IsSensitive, AppUserId (null ise global)
- AppUser + AppUserProfile: Constructor + private set + domain metodları
- TÜM Interface'ler: IAppDbContext (DbSet'ler), IRepository<T>, IUnitOfWork (her entity için repo property), HER ENTİTY İÇİN ÖZEL REPO INTERFACE, IPasswordHasher, ITokenService, IUserActivityLogger, IEmailService

### Adım 2: Application Katmanı
- DependencyInjection.cs: AddMediatR, AddFluentValidation, AddAutoMapper, ValidationBehavior pipeline
- Exception sınıfları, MappingProfile
- DTO'lar, Validator'lar, Command/Query, Handler'lar (IUnitOfWork üzerinden repository kullan)

### Adım 3: Infrastructure Katmanı
- AppDbContext: DbSet'ler + **SaveChangesAsync OVERRIDE**
  - ChangeTracker'dan Added/Modified/Deleted BaseEntity'leri bul → tarih/sahip set et
  - Soft delete uygula (Deleted state'i Modified + Status=Deleted yap)
  - Değişen property'leri bul → AppUserActivited kaydı oluştur (EntityCreated/Updated/Deleted)
  - Log kayıtlarının kendisini tekrar loglama (AppUserActivited ve SystemLog'u takip dışı bırak)
- Entity Configurations, Repository implementasyonları (Generic + Özel), UnitOfWork
- Security: PasswordHasher (BCrypt workFactor:11), JwtTokenService, EncryptionHelper, HashHelper, UserActivityLogger
- EmailService (MailKit), SerilogConfigurator (Console + MSSqlServer sink, SystemLogs tablosu)
- Infrastructure DependencyInjection: Tüm servisleri register et

### Adım 4: API Katmanı
- IEndpoint interface + EndpointExtensions (Reflection ile tüm IEndpoint'leri tara)
- AuthEndpoints, UserProfileEndpoints, SystemLogsEndpoints
- ExceptionHandlingMiddleware (try-catch → SystemLog + Serilog.Error)
- Api DependencyInjection (Authentication/JWT, Authorization, Swagger, CORS, Serilog RequestLogging)
- Program.cs: 20 satırdan az olsun
- appsettings.json: Tüm config section'larını ekle

### Adım 5: Kök Dökümanlar
- PROJECT_TEMPLATE_ROADMAP.md: Mevcut mimari, sonraki adımlar (Roller, Redis, Hangfire, TOTP, RateLimit vb.), Yeni Entity Ekleme Rehberi (7 adım)
- AI_RULES_FOR_THIS_REPO.md: Katman referans kuralları, Entity kuralları (private set), Repository kuralları (özeldir, doğrudan DbContext YOK), CQRS kuralları, Log kuralları, Validation kuralları, Endpoint kuralları, ANTI-PATTERN listesi (NE YAPILMAYACAĞI)

### Adım 6: Build Doğrulaması
- `dotnet restore` + `dotnet build -warnaserror`
- Katman referanslarını kontrol et: Domain hiç ProjectReference almaz!
- Program.cs satır sayısı < 20

---

## Dependencies and Considerations

### SaveChanges Override Kritik Detay
- Activity log kaydı (AppUserActivited) kendisi için tekrar activity log yaratmamalı → ignore et
- ChangeTracker.EntityState, OriginalValue/CurrentValue karşılaştırması ile sadece değişen property'leri Description'a yaz (JSON formatında)
- CreatedBy/ModifiedBy değeri varsa onu AppUserId olarak kullan, yoksa aktiviteyi "System" olarak işaretle (yine de kaydedilsin)

### Serilog
- `builder.Host.UseSerilog((ctx, cfg) => SerilogConfigurator.Configure(cfg, ctx.Configuration))`
- Serilog Sink MSSQL Server otomatik SystemLogs tablosuna yazar → ayrıca ISystemLogRepository üzerinden de yazabiliriz (redundancy)

### Riskler
- .NET 11 Preview Uyumsuzluğu: Paketleri preview sürüm kullan, sorun olursa net10.0'a düş
- SaveChanges Override Infinite Loop: AppUserActivited + SystemLog entity'lerini auditden çıkar
- Katman Referans Döngüsü: Elle kontrol et - Domain hiç project reference almaz
- Security: JWT Secret'i User Secrets / Key Vault kullan, production'da hardcode YOK
- MailKit Lisans: Ticari projelerde alternatif olarak System.Net.Mail veya SendGrid notunu roadmap'e yaz
