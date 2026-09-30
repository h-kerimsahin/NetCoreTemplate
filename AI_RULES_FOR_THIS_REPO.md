# AI Rules for NetCoreTemplate Repository

> BU DOKÜMAN, BU REPOSITORY ÜZERİNDE ÇALIŞAN TÜM YAPAY ZEKA (Trae, GitHub Copilot, ChatGPT, CodeWhisperer vb.) araçları İÇİN ZORUNLU KURALLARDIR. Herhangi bir kod değişikliği, yeni feature ekleme veya refactoring yapmadan ÖNCE bu kuralları OKUYUN ve UYUN. İHLAL EDİLEN KURAL OLMAMALIDIR.

---

## 🎯 Nihai Hedef
Bu repository, **TEMİZ KATMANLI MİMARİ + CQRS + UnitOfWork + Repository Pattern** kullanan .NET 11 şablonudur. Sonraki projelerde bu şablonu temel alarak hızlıca başlanması hedeflenir. Bu nedenle MİMARİ BOZULMAMALI, KALIPLAR DEĞİŞTİRİLMEMELİDİR.

---

## KURAL 1: KATMAN REFERANSLARI KURALLARI (EN ÖNEMLİ - KESİNLİKLE UY)

### Referans Matrisi (SADECE bunlara İZİN VAR)
| Proje | Hangi projeleri referans alabilir | HANGİLERİNİ ALAMAZ |
|-------|-----------------------------------|----------------------|
| **NetCoreTemplate.Domain (EN ÜST)** | ❌ **HİÇBİR PROJEYİ REFERANS ALMAZ!** | Tümü (❌ Application ❌ Infrastructure ❌ Api) |
| **NetCoreTemplate.Application** | ✅ SADECE `NetCoreTemplate.Domain` | ❌ Infrastructure ❌ Api (Bunları bildiğini sanma!) |
| **NetCoreTemplate.Infrastructure** | ✅ SADECE `NetCoreTemplate.Application` | ❌ Api ❌ Domain (DOĞRUDAN Domain Referansı YOK! Application üzerinden domain interface'lerini zaten kullanıyor) |
| **NetCoreTemplate.Api (Sunum)** | ✅ SADECE `NetCoreTemplate.Infrastructure` | ❌ Application ❌ Domain'den DOĞRUDAN REFERANS ALMAYIN! |

### Ceza Maddesi:
❌ Domain csproj içinde **HERHANGİ** bir `<ProjectReference>` görürsen HEMEN DUR ve hatayı bildir. Domain projesinde **SADECE NuGet paketleri** olabilir (örn: Microsoft.EntityFrameworkCore interface için zorunlu, o kabul).

✅ ÖRNEK DOĞRU:
- Api → Infrastructure → Application → Domain (tek yönlü, zincir, DÖNGÜ YOK
- Bu zincirin DIŞINDA referans YOK.

---

## KURAL 2: ENTİTY (Domain/Entities) KURALLARI

### Zorunlu
1. **TÜM entity'ler `BaseEntity.cs`'den kalıtım alır** (Id, CreatedDate/ModifiedDate/DeletedDate, CreatedBy/ModifiedBy/DeletedBy, Status (soft delete)
2. **Property setter'ları MUTLAKA `private` veya `protected` OLMALIDIR** (public set YOK! Kapsülleme prensibi
   - Doğru: `public string Name { get; private set; }`
   - ❌ Yanlış: `public string Name { get; set; }`
3. **Non-nullable string/reference property'leri MUTLAKA Constructor'da set et** (veya `= null!` ama constructor TERCIH EDİLİR)
4. **Entity durumu SADECE DOMAIN METOTLARI ile değiştirilir** (property setter private olduğundan zaten mecbur)
   - Örn: `user.UpdatePassword(hash)` DOĞRU. `user.PasswordHash = "abc"` ❌ YANLIŞ (derleme hatası vermeli
5. **Yeni entity için enum gerekiyorsa `Domain/Enums/` klasörüne ekle** (entity yerine enum kullan)
6. **Navigation property'leri `private set` veya `init` ile tanımla**, `ICollection<T>` ise `= new HashSet<T>()` ile initialize et

### Anti-Pattern'ler (YAPMA)
❌ Entity içinde `using Microsoft.EntityFrameworkCore;` EF Core attribute (Key, ForeignKey) kullanma!
→ Onun yerine **IEntityTypeConfiguration** kullan (Infrastructure/Persistence/Configurations/)
❌ Entity içinde DTO veya API request/response nesneleri kullanma
❌ Entity içinde business logic dışında (örneğin JWT token üretme, email gönderme) yapma! Onlar Application veya Infrastructure katmanında olur

---

## KURAL 3: REPOSITORY + UNITOFWORK KURALLARI

### Repository Kuralları (ÇOK ÖNEMLİ!)
1. **HER ENTİTY İÇİN MUTLAKA ÖZEL REPO INTERFACE + IMPL OLMALI**
   - Entity: `Product` → Interface: `IProductRepository : IRepository<Product>`
   - Impl: `ProductRepository : Repository<Product>, IProductRepository`
   - Asla entity için generic IRepository<T> kullan YOK! Her zaman özeli repo interface'inden DI
2. **Generic base repo (IRepository<T>) yalnızca genel CRUD işlemlerini içerir** (GetAll, GetById, GetWhere, Add, Update, Delete, PermanentDelete, Count, Any, FirstOrDefault
3. **Özel repo (IProductRepository), entity'ye özel sorgular içerir**
   - Örn: `Task<Product?> GetBySkuAsync(string sku);` veya `Task<List<Product>> GetActiveProductsWithCategoryAsync()
4. **Repository içinde LINQ Sorguları ASLA `AsNoTracking()` unutma** (performans)
5. **Repository içinde Include/ThenInclude ile ilişkileri çek** ama over-engineering yapma

### UnitOfWork Kuralları
1. **IUnitOfWork içinde HER ENTİTY İÇİN ÖZEL REPO property'si olmalı**
   - `IAppUserRepository AppUsers { get; }`, `IProductRepository Products { get; }` vb.
   - Property isimleri **çoğul** olsun (AppUsers, Products)
2. **UnitOfWork SADECE `SaveChangesAsync()` ve property'leri içerir** (transaction yönetimi ileride eklenebilir
3. **Lazy init kullanılır:** property ilk defa erişilince new'lenir (performans, bellek

### Application Katmanı Repository Kullanımı (HANDLER'LAR İÇİN)
✅ **HANDLER'LARDA SADECE IUnitOfWork KULLANILIR, DOĞRUDAN IAppDbContext KULLANILMAZ!**
✅ Doğru:
```csharp
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _uow; // SADECE UOW!
    public CreateProductHandler(IUnitOfWork uow) => _uow = uow;
    
    public async Task<ProductDto> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = new Product(cmd.Name, cmd.Price);
        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct); // KAYDETMEK İÇİN UOW KULLAN!
        return _mapper.Map<ProductDto>(product);
    }
}
```
❌ **YAPMA! Doğrudan AppDbContext enjekte etme:**
```csharp
public class CreateProductHandler(AppDbContext context) // ❌ HATA!
```

---

## KURAL 4: CQRS (Application/Features) KURALLARI

### Her İş İçin Aşağıdaki Pattern'i Uygula:
```
Application/Features/[FeatureName]/
├── Commands/ (Değişiklik yapanlar: Create, Update, Delete, Enable, Verify)
│   └── [ActionName]/
│       ├── [ActionName]Command.cs (IRequest<TResponse> implement, record)
│       ├── [ActionName]CommandHandler.cs (IRequestHandler implementasyon)
│       └── [ActionName]CommandValidator.cs (FluentValidation.AbstractValidator)
└── Queries/ (Sadece Okuma: GetById, GetAll, Search, Me)
    └── [QueryName]/
        ├── [QueryName]Query.cs (IRequest<TResponse> implement)
        └── [QueryName]QueryHandler.cs (IRequestHandler implementasyon)
```

### Zorunlu
1. **Her Command/Query bir RECORD** olmalı (immutable)
   - Örn: `public record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;`
2. **TÜM Command'lar için FLUENTVALIDATION VALIDATOR YAZ** (Query'ler için de gerekliyse yaz
3. **Handler içinde iş kuralları: Repository'ler üzerinden data al, state değiştir, SaveChangesAsync, DTO dön
4. **Handler'da HttpContext, HttpResponse, IResult üretme** (Endpoint pattern'i ile Api katmanında yapılır
5. **Dto DÖNÜŞÜMÜ İÇİN HER ZAMAN AutoMapper kullan, ELLE YAZMA (MappingProfile'da tanımlı olmalı
6. **Handler içinde exception fırlatabilirsin** (NotFoundException, BusinessException, ValidationException, UnauthorizedException) - Middleware otomatik yakalar

### Anti-Pattern (YAPMA)
❌ Bir handler'da 3'ten fazla iş yapma (Single Responsibility). İstersen 2 ayrı Command/Handler ayır.
❌ Handler içinde doğrudan SmtpClient veya BCrypt çağırma → Onun yerine IEmailService, IPasswordHasher kullan (Interface segregation
❌ Mapping Profile'ı unutma → AutoMapper MappingException hatası verir

---

## KURAL 5: LOG KURALLARI

### 3 Seviye Loglama Mekanizması Var (HEPSİNİ DE DOĞRU YERDE KULLAN)

| Log Türü | Nerede Kullanılır | Entity/Class | Nasıl Eklenir |
|----------|-------------------|--------------|---------------|
| **1. Kullanıcı Aktivite Logu** (Login, Şifre değiştirme, Profil güncelleme, Entity CRUD) | Kullanıcı bazlı her işlem | `AppUserActivited` tablosu, `UserActivityType` enum | ✅ Otomatik (SaveChanges.ProcessAudit Entity Create/Update/Delete için) + ✅ Manuel (IUserActivityLogger.LogAsync Login/Logout vb.) |
| **2. Sistem Hatası / Application Log** (Exception, EF hatası, Background job hatası) | Tüm Exception'lar + Warning | `SystemLog` tablosu, `SystemLogLevel` enum | ✅ Otomatik (ExceptionHandlingMiddleware tümünü yakalar) + ✅ Manuel (ISystemLogRepository üzerinden) |
| **3. Serilog Yapılandırılmış Log** (Debug/Info/Warning/Error structured | Her yer | Serilog + Console + **SystemLogs** (aynı MSSQL Sink üzerinden | `ILogger<T>` DI ile al → `_logger.LogInformation/Warning/Error` |

### Kullanım Kuralları
- **Doğrudan `Serilog.Log` static kullanmak yerine `ILogger<T>` tercih et** (test edilebilirlik için)
- **User aktivitesi için** Kayıt işlemi → `IUserActivityLogger` (AppUserActivited'e yazar) + SaveChanges otomatik
- **Exception için** → İçinde middleware zaten SystemLog tablosuna + Serilog Error yazıyor, ekstradan yapmana GEREK YOK
- **Entity CRUD logu için** → SaveChanges.ProcessAudit zaten otomatik olarak AppUserActivited'e yazıyor, ELLE YAZMA!
- **Log mesajlarını İngilizce yaz** (standardizasyon için)

---

## KURAL 6: VALIDATION KURALLARI

### 2 Seviye Doğrulama Var
1. **Input Validation (FluentValidation - Application Katmanı, Command/Query için)**
   - Kullanıcı girdisi doğrulaması: Boş olmaması, uzunluk, regex, email format, vs.
   - Validator sınıfı → ValidationBehavior pipeline'dan otomatik çalışır
   - Hata → ValidationException → Middleware 400 Bad Request döner
2. **Business Rule Validation (Domain Entity ve Handler içinde)**
   - İş kuralı: "Stok ürününden az olamaz", "Aynı email ile kayıt olamaz"
   - Handler içinde kontrol edip → BusinessException fırlat (400 Bad Request)
   - Entity içinde: `if (price < 0) throw new InvalidOperationException()` (daha iyi olur domain katmanında

### Anti-Pattern (YAPMA)
❌ Endpoint (API) katmanında doğrulama yapma → (örn `if (string.IsNullOrEmpty(cmd.Name))` → Bunu yapma Validation yazmak yerine!
❌ Validator içinde DB'den data okumaya çalışma (örn: `userManager.FindByNameAsync`) iş kuralı Handler'da yapılır.
❌ Entity doğrulamasını public setter'ları açıp UI'da yapma! (Setter'lar private kural 2'de var)

---

## KURAL 7: ENDPOINT (Presentation/Api) KURALLARI

### Minimal API + IEndpoint Pattern KULLANILIR (MVC Controller değil!)
1. **Yeni bir endpoint grubu için Yeni Sınıf:** `[Feature]Endpoints.cs` (örn: ProductsEndpoints.cs
2. **IEndpoint interface'ini IMPLEMENT ET:** `public class ProductsEndpoints : IEndpoint`
3. **`Map(IEndpointRouteBuilder app)` metodunu override et:**
   ```csharp
   var group = app.MapGroup("api/products").WithTags("Products").WithOpenApi().RequireAuthorization();
   group.MapGet("{id}", async ...); // ...
   ```
4. **Endpoint handler içinde SADECE Mediator.Send çağır** ve Result döner:
   ```csharp
   group.MapPost("", async (CreateProductDto dto, ISender sender) =>
   {
       var cmd = new CreateProductCommand(dto.Name, dto.Price);
       var result = await sender.Send(cmd);
       return Results.Created($"/api/products/{result.Id}", result);
   });
   ```
5. **Authorization:** Group seviyesinde `.RequireAuthorization()` veya tek endpoint'te veya `[AllowAnonymous]`
6. **MapEndpoints() çağrısı Program.cs'de OTOMATİK** reflection ile tüm IEndpoint sınıflarını tarar, ELLE endpoint ekleme Map* yapma

### Program.cs Kuralı
✅ **Program.cs TOPLAM 20 SATIRI GEÇMEMELİDİR!** (Şu an 18 satır)
- BÜTÜN servis registration → `builder.ConfigureServices()` extension method
- BÜTÜN middleware pipeline → `app.ConfigurePipeline()` extension method
- Program.cs içinde MapPost/MapGet GET/YAZMA!! (Endpoint sınıflarında yapılır

### Anti-Pattern (YAPMA)
❌ Controller tabanlı Web API kullanmak (MVC Controller) - Minimal API + IEndpoint şablon standardı
❌ Program.cs içinde service registration veya endpoint map yazmak - Okunmaz, şişer
❌ Endpoint handler'ında uzun iş kodu yazmak - Hepsi Handler'a gitsin (Mediator pattern)

---

## KURAL 8: SECURITY (GÜVENLİK) KURALLARI

### Password Hash
✅ **BCrypt.Net-Next WorkFactor = 11** kullan (PasswordHasher.cs'de var)
❌ MD5, SHA1, SHA256 tek başına password hash için kullanmak (Tuzsuz, saldırıya açık) ❌

### JWT Token
✅ Access Token = 15dk (kısa ömür) | Refresh Token 14 gün (uzun, tek kullanımlık Rotate)
✅ RefreshToken DB'de HASHLENMIŞ saklanır (raw token DB'de YOK!)
✅ JWT Secret Key appsettings'te DEĞİL → Production'da User Secrets veya Azure Key Vault
❌ JWT Token içinde PasswordHash, SecurityStamp gibi hassas bilgi saklama!

### Şifreleme Yardımcıları (Kullanım Alanları):
- **EncryptionHelper.AES-256** → Hassas config, connection string şifreleme (simetrik)
- **HashHelper.SHA256** → Data integrity, checksum, benzersiz ID üretmek için
- **HashHelper.HMACSHA256** → Webhook imzası, API doğrulama için (key ile hash)
❌ Bu yardımcıları şifre hash için KULLANMA (PasswordHasher kullan)

---

## KURAL 9: YENİ PACKET / DEPENDENCY EKLEME KURALLARI

Bir NuGet paketi eklemeden ÖNCE:
1. Hangi katmana ait olduğunu belirle:
   - **Domain:** SADECE interface için gerekli paketler (örn: EF Core interface için - kabul)
   - **Application:** Pattern paketleri (MediatR, FluentValidation, AutoMapper)
   - **Infrastructure:** Implementation paketleri (EF Core Provider, BCrypt, MailKit, JWT, Serilog
   - **Api:** Presentation concern paketleri (JwtBearer, Swagger, Serilog.AspNetCore)
2. Paketin lisans kontrolü yap: MIT / Apache 2.0 iyi, GPL/AGPL ticari projelerde riskli (örn: MailKit 4.x GPL, ticari lisans gerekir dikkat et!)
3. Paketin sürümü .NET 11 ile uyumlu mu? (preview olunca dikkat et!)
4. Projeye özgü bir paket mi (template olması için çok spesifik paketleri ekleme ❌).

---

## KURAL 10: NAMING CONVENTIONS (İSİMLENDİRME STANDARDI)

| Öğe | Kural | Örnek |
|-----|-------|-------|
| Entity | Singular PascalCase | AppUser, AppUserProfile, Product, Order |
| DbSet | Plural PascalCase | AppUsers, Products, Orders |
| Repository Interface | I + EntityName + Repository | IAppUserRepository, IProductRepository |
| Repository Implementation | EntityName + Repository | AppUserRepository, ProductRepository |
| DTO (Read) | EntityName + Dto (record) | ProductDto, UserProfileDto |
| Request DTO | Operation + RequestDto | CreateProductRequestDto, LoginRequestDto |
| Command (CQRS) | Operation + Command | CreateProductCommand, LoginCommand |
| Query (CQRS) | Operation + Query | GetProductByIdQuery, MeQuery |
| Handler (CQRS) | Command/Query Name + Handler | CreateProductCommandHandler, MeQueryHandler |
| Validator (Fluent) | Command/Query Name + Validator | CreateProductCommandValidator |
| Endpoint Sınıfı | Feature + Endpoints | AuthEndpoints, ProductsEndpoints |
| Enum | PascalCase | UserActivityType, SystemLogLevel |
| Enum Değer | PascalCase | LoginFailed, EntityCreated |
| Private Alan | _camelCase | _unitOfWork, _passwordHasher |
| Async Metod | Async son eki | GetByIdAsync(), SaveChangesAsync() |

---

## 🚫 KESİNLİKLE YAPILMAMASI GEREKENLER (ANTI-PATTERN BLACKLIST)

1. ❌ **Domain projesine ProjectReference eklemek** (Domain EN ÜST, hiç kimseyi referans almaz!)
2. ❌ **Application Handler içinde doğrudan AppDbContext kullanmak** (IUnitOfWork KULLANILACAK)
3. ❌ **Entity property'lerini public set yapmak** (private/protected set ZORUNLU)
4. ❌ **Yeni entity için özel repository yazmadan IRepository<T> kullanmak** (ÖZEL REPO ZORUNLU)
5. ❌ **Program.cs'i şişirmek** (20 satırdan yukarı çıkarsa ayıp!)
6. ❌ **Endpoint'leri Program.cs içinde MapPost/MapGet ile yazmak** (IEndpoint sınıflarında yazılacak)
7. ❌ **Doğrudan `new AppUser(...)` yerine Entity'yi UI'da new'leyip property'leri elle set etmek** (Domain metodu kullan!)
8. ❌ **Password hash için MD5/SHA/SHA256** (SADECE BCrypt)
9. ❌ **BCrypt WorkFactor < 10** (11 ideal)
10. ❌ **SaveChanges'ta ProcessAudit'i kaldırmak veya bozmak** (Kullanıcı aktivite logu için kritik)
11. ❌ **AppUserActivited ve SystemLog entity'leri için audit log yazmak** (infinite loop olur)
12. ❌ **Katmanlar arası DTO yerine doğrudan Entity kullanmak** (DTO / record / AutoMapper MECBURİ)
13. ❌ **Handler içinde HttpContext veya IResult üretmek** (Endpoint katmanında olur, Handler saf olmalı)
14. ❌ **Exception handling için her endpoint'te try-catch yazmak** (Global Middleware otomatik yapıyor)
15. ❌ **Custom bir Repository pattern icat etmek** (Mevcut IRepository<T> + özel repo pattern'ını BOZMA)
16. ❌ **UnitOfWork yerine doğrudan Repository.SaveChanges çağırmak** (UoW.SaveChangesAsync ZORUNLU)
17. ❌ **Swagger Security Scheme kaldırmak** (JWT Authorize butonu çalışmalı)
18. ❌ **Serilog yerine kendi logger'ını yazmak** (Standart Serilog + ILogger<T> kullan)
19. ❌ **Yeni feature için 7 adım Yeni Entity Ekleme Rehberi (ROADMAP.md madde 4)'ü atlamak** (her adımı izle)
20. ❌ **MailKit GPL lisansını unutmak** (Production'da MIT lisanslı alternatif kullanma gerekebilir - Roadmap not edildi)

---

## ✅ YAPILMASI GEREKENLER / CHECKLIST (Yeni Feature Eklerken)

Önce bu checklist'i geç, sonra commit et:
- [ ] Katman referansları doğru mu? (Api → Infrastructure → Application → Domain döngüsüz
- [ ] Yeni entity BaseEntity'den kalıtım alıyor mu? Property setter private/protected?
- [ ] Entity için IEntityRepository + Impl var mı? IUnitOfWork'da property var mı?
- [ ] EF Configuration (IEntityTypeConfiguration) yazıldı mı?
- [ ] Feature/[X] için Command/Query + Handler + Validator yazıldı mı?
- [ ] DTO'lar ve MappingProfile güncellendi mi?
- [ ] Endpoints/IEndpoint sınıfı yazıldı, Program.cs MAP yazılmadı mı?
- [ ] Kaydedilen entity için SaveChanges audit log (AppUserActivited) otomatik düşüyor mu?
- [ ] Exception için SystemLog tablosuna kayıt düşüyor mu? (Middleware test)
- [ ] Security: Password için BCrypt, JWT secret Key Vault/User Secrets
- [ ] Build: `dotnet build -warnaserror` 0 hata 0 uyarı
- [ ] ROADMAP.md ve YANI BU DOSYAYI (AI_RULES_FOR_THIS_REPO.md) güncellemeyi unuttun mu?

---

## 📌 Referans Belgeler
- Proje Yol Haritası: [PROJECT_TEMPLATE_ROADMAP.md](PROJECT_TEMPLATE_ROADMAP.md)
- Katmanlı Mimari: https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
- CQRS + MediatR: https://github.com/jbogard/MediatR
- FluentValidation: https://docs.fluentvalidation.net/
- Clean Architecture: https://github.com/jasontaylordev/CleanArchitecture (referans proje)

---

**Version**: 1.0 | **Tarih**: 2026-09-30 | **Yazar**: NetCoreTemplate
