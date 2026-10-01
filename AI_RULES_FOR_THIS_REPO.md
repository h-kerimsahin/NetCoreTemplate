# NetCoreTemplate - Kural Seti ve Proje Yol Haritası (TEK KURAL DOSYASI)

> BU DOKÜMAN TEK KAYNAK dosyasıdır: hem yapay zeka (Trae, Copilot, ChatGPT vb.) için ZORUNLU kurallar, hem de proje geliştirme yol haritası, yeni entity ekleme rehberi ve teknik referans TEK DOSYADA bulunur. İKİNCİ BİR MD DOSYASI OLUŞTURMAYIN. Herhangi bir kod değişikliği, yeni feature ekleme veya refactoring yapmadan ÖNCE bu kuralları OKUYUN ve UYUN.

---

## 🎯 Nihai Hedef
Bu repository, **TEMİZ KATMANLI MİMARİ + CQRS + UnitOfWork + Repository Pattern + ApiResponse Standard + Sayfalama + Idempotency** kullanan .NET 11 şablonudur. Sonraki projelerde bu şablonu temel alarak hızlıca başlanması hedeflenir. Bu nedenle MİMARİ BOZULMAMALI, KALIPLAR DEĞİŞTİRİLMEMELİDİR.

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
- Api → Infrastructure → Application → Domain (tek yönlü, zincir, DÖNGÜ YOK)
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
   - Örn: `Task<Product?> GetBySkuAsync(string sku);` veya `Task<List<Product>> GetActiveProductsWithCategoryAsync()`
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
│       ├── [ActionName]Command.cs (IRequest<ApiResponse<TResponse>> implement, record)
│       ├── [ActionName]CommandHandler.cs (IRequestHandler implementasyon - ApiResponse<T> döner)
│       └── [ActionName]CommandValidator.cs (FluentValidation.AbstractValidator)
└── Queries/ (Sadece Okuma: GetById, GetAll, Search, Me)
    └── [QueryName]/
        ├── [QueryName]Query.cs (IRequest<ApiResponse<TResponse>> veya IRequest<PagedResponse<T>> implement)
        └── [QueryName]QueryHandler.cs (IRequestHandler implementasyon)
```

### Zorunlu
1. **Her Command/Query bir RECORD** olmalı (immutable)
   - Doğru: `public record CreateProductCommand(string Name, decimal Price) : IRequest<ApiResponse<ProductDto>>;`
   - ❌ Eskisi (YAPMA): `IRequest<ProductDto>` → TÜM yanıtlar ApiResponse<T> veya PagedResponse<T> ile SARILMIŞ olmalı!
2. **TÜM Command'lar için FLUENTVALIDATION VALIDATOR YAZ** (Query'ler için de gerekliyse yaz)
3. **Handler içinde iş kuralları: Repository'ler üzerinden data al, state değiştir, SaveChangesAsync, DTO'yu ApiResponse.Success() ile sar → return**
4. **Handler'da HttpContext, HttpResponse, IResult üretme** (Endpoint pattern'i ile Api katmanında yapılır
5. **Dto DÖNÜŞÜMÜ İÇİN HER ZAMAN AutoMapper kullan, ELLE YAZMA (MappingProfile'da tanımlı olmalı**
6. **Handler içinde exception fırlatabilirsin** (NotFoundException, BusinessException, ValidationException, UnauthorizedException) - Middleware otomatik yakalar → ApiResponse.Fail üretir
7. **TÜM DÖNÜŞLERDE ApiResponse KULLANILIR (ZORUNLU)**
   - Başarılı: `return ApiResponse.Success(dto, StatusCodes.Status200OK, "İşlem başarılı");`
   - Sayfalı liste: `return ApiResponse.Paged(items, pageNumber, pageSize, totalCount, StatusCodes.Status200OK);`
   - Void/Unit command (ör: Logout): `return ApiResponse.Success(Unit.Value, StatusCodes.Status204NoContent, "Çıkış başarılı");`

### Anti-Pattern (YAPMA)
❌ Bir handler'da 3'ten fazla iş yapma (Single Responsibility). İstersen 2 ayrı Command/Handler ayır.
❌ Handler içinde doğrudan SmtpClient veya BCrypt çağırma → Onun yerine IEmailService, IPasswordHasher kullan (Interface segregation
❌ Mapping Profile'ı unutma → AutoMapper MappingException hatası verir
❌ **Handler'da düz DTO (ApiResponse'siz) return etme** - ZORUNLU SAR

---

## KURAL 5: API RESPONSE STANDARDI (YENİ - ZORUNLU)

TÜM HTTP yanıtları **TEK FORMATTA** döner: `ApiResponse` veya türevleri (`ApiResponse<T>`, `PagedResponse<T>`). Başarı ve hata AYNI JSON şemasını paylaşır.

### Standart JSON Şeması:
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "İşlem başarılı.",
  "errors": null,
  "timestamp": 1760000000,
  "data": { ... }
}
```

### Zorunlu
1. **Başarı yanıtı →** `ApiResponse.Success<T>(data, 200, "mesaj")` veya `ApiResponse.Paged<T>(...)` (sayfalı listeler)
2. **Hata yanıtı →** Hiçbir yerde manual `new { error = ... }` veya anonim tip DÖNME! Middleware exception'ları `ApiResponse.Fail(...)` ile üretir, sen de manuel hata dönersen aynı formatı kullan.
3. **ExceptionMiddleWare tek kaynaktır.** Tüm NotFound, Validation, Business, Unauthorized, 500 hataları middleware'de → ApiResponse.Fail formatına çevrilir + SystemLog tablosuna kaydedilir. ELLE endpoint içine try-catch yazma.
4. **JSON serileştirme:** Api katmanında `Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = CamelCase, DefaultIgnoreCondition = WhenWritingNull }, statusCode: response.StatusCode)` pattern'ını kullan.
5. **PagedResponse<T> (sayfalı)** sadece liste dönen GET/QUERY isteklerinde kullanılır. `pageNumber, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage` alanlarını içerir ZORUNLU.

### Anti-Pattern (YAPMA)
❌ `Results.Ok(duzDto)` düz DTO doğrudan döndürme → `ApiResponse.Success(duzDto)` ile SAR!
❌ `Results.BadRequest(new { message = "..." })` anonim tip → Kullanma, exception fırlat, middleware çevirir ya da ApiResponse.Fail kullan
❌ Her endpointin farklı format dönmesi (şişman ürün örneği `{ status: "ok", result: ... }` gibi) → TEK FORMAT ZORUNLU.

---

## KURAL 6: SAYFALAMA (PAGINATION) STANDARDI (YENİ - ZORUNLU)

Liste dönen BÜTÜN endpoint/query'ler (ör: GetAllProducts, GetSystemLogs, GetUserActivities) **MUTLAKA sayfalı (PagedResponse<T>)** olmalıdır.

### Standart Sınıflar:
- `Application/DTOs/Common/PagedRequest.cs` → abstract base. `PageNumber (default=1), PageSize (default=10 veya 20), SearchTerm, OrderBy, OrderByDescending` alanları.
- `ApplyPagination<T>()` → IQueryable ve IEnumerable için extension metod. `skip = (pageNumber-1)*pageSize`

### Zorunlu
1. Yeni bir liste Query'si oluşturduğunda `public record GetProductsQuery(int PageNumber = 1, int PageSize = 20, ...) : PagedRequest, IRequest<PagedResponse<ProductDto>>;` kalıbını kullan (PagedRequest'ten kalıtım al)
2. **Toplam sayıyı (totalCount) MUTLAKA sorgula:** `totalCount = await _uow.Products.GetAll().CountAsync(ct);` (filtre varsa filtre sonrası count)
3. **Dönüş:** `return ApiResponse.Paged(items, request.PageNumber, request.PageSize, totalCount);`
4. **PageSize upper bound:** Client'ın 10000 eleman tek sefer istemesini engelle — Validator ile PageSize ≤ 200 sınırla.
5. **Filtre + sıralama + sayfalama sırası → Önce Where, OrderBy, SONRA Count(), SONRA ApplyPagination() (SKIP+TAKE)'in peşi sıra uygulanması (performans için çok kritik)

### Anti-Pattern
❌ 10 bin satırı tek endpointte dönmek (performance ölüm!) → SAYFALA
❌ totalCount'u yazmak yerine hardcode 0/1 bas → DataTable/Grid kırılır

---

## KURAL 7: IDEMPOTENCY (MÜKERRER İŞLEM ENGELLEME) MEKANİZMASI (YENİ - ZORUNLU)

Kullanıcı aynı butona (submit/ödeme/kayıt) 1 saniyede 10 kere basarsa YA DA network retry sonrası AYNI POST/PUT isteği 2 kez sunucuya gelirse, **İŞLEMİ 1 KEZ UYGULA, 2. isteğe ÖNCEKI BAŞARILI YANITI DÖN**.

### Mekanizma:
1. **Header:** Client POST/PUT/DELETE isteğinde `X-Idempotency-Key: benzersiz-guid` gönderir. (Gönderilmezse geleneksel davranış, idempotency uygulanmaz — geriye dönük uyumluluk)
2. **Cache:** `IMemoryCache` (InMemory, sunucu başına; production'da IDistributedCache → Redis ile değiştirilebilir. Interface üzerinden tasarlandı, kolayca değiştirilir.
3. **Endpoint Filter:** `IdempotencyEndpointFilter` (IEndpointFilter) tüm POST/PUT/DELETE endpointlerine `.AddEndpointFilter<IdempotencyEndpointFilter>()` ile eklenir.
4. **Key alanı:** `{UserId veya "anonymous"}|{HTTPMETHOD}|{PATH}|{IdempotencyKey}` hashlenmiş veya bileşik key ile cache'lenir. (Userlar arası karışmaz, endpointler arası karışmaz.)
5. **Sliding Expiration:** Default 120 dakika (2 saat). `appsettings.json: Idempotency:SlidingExpirationMinutes` ile konfigüre edilir.

### Zorunlu
1. **YENİ BİR Feature (Products vb.) için POST/PUT/DELETE endpointleri oluşturduğunda:** Mutlaka endpoint grubuna `.AddEndpointFilter<IdempotencyEndpointFilter>()` ekle.
2. **READ-only endpointler (MapGet) için ekleme** (idempotent GET'ler ZATEN idempotent'tir, ek maliyet yaratma)
3. **IdempotencyService Domain Interface üzerinden yönetilir:** `Domain/Interfaces/IIdempotencyService` → InMemory/Redis değiştirilebilir (Dependency Injection ile Infrastructure katmanı).
4. **Cache'te saklama JSON serialized response body:** 2xx başarılı yanıtları saklar (5xx/4xx hatalarını saklama — tekrar denenebilir)

### Anti-Pattern
❌ Payment gibi kritik endpointlerde filter'ı unutmak.
❌ X-Idempotency-Key boşsa işi exception ile reddetmek → GERİYE DÖNÜK UYUMLULUK: boşsa normal davran, cache kontrolü atla.
❌ 60 saniye gibi kısa cache süresi → retry senaryolarında işe yaramaz. Minimum 30dk, ideal 2 saat.

---

## KURAL 8: LOG KURALLARI

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

## KURAL 9: VALIDATION KURALLARI

### 2 Seviye Doğrulama Var
1. **Input Validation (FluentValidation - Application Katmanı, Command/Query için)**
   - Kullanıcı girdisi doğrulaması: Boş olmaması, uzunluk, regex, email format, vs.
   - Validator sınıfı → ValidationBehavior pipeline'dan otomatik çalışır
   - Hata → ValidationException → Middleware **ApiResponse.Fail(400, errorsDict)** döner
2. **Business Rule Validation (Domain Entity ve Handler içinde)**
   - İş kuralı: "Stok ürününden az olamaz", "Aynı email ile kayıt olamaz"
   - Handler içinde kontrol edip → BusinessException fırlat (400 Bad Request, ApiResponse formatında)
   - Entity içinde: `if (price < 0) throw new InvalidOperationException()` (daha iyi olur domain katmanında

### Anti-Pattern (YAPMA)
❌ Endpoint (API) katmanında doğrulama yapma → (örn `if (string.IsNullOrEmpty(cmd.Name))` → Bunu yapma Validation yazmak yerine!
❌ Validator içinde DB'den data okumaya çalışma (örn: `userManager.FindByNameAsync`) iş kuralı Handler'da yapılır.
❌ Entity doğrulamasını public setter'ları açıp UI'da yapma! (Setter'lar private kural 2'de var)

---

## KURAL 10: ENDPOINT (Presentation/Api) KURALLARI

### Minimal API + IEndpoint Pattern KULLANILIR (MVC Controller değil!)
1. **Yeni bir endpoint grubu için Yeni Sınıf:** `[Feature]Endpoints.cs` (örn: ProductsEndpoints.cs
2. **IEndpoint interface'ini IMPLEMENT ET:** `public class ProductsEndpoints : IEndpoint`
3. **`Map(IEndpointRouteBuilder app)` metodunu override et:**
   ```csharp
   var group = app.MapGroup("api/products").WithTags("Products").RequireAuthorization();
   group.MapGet("{id}", async ...);
   group.MapPost("", async ...).AddEndpointFilter<IdempotencyEndpointFilter>(); // ← Idempotency (KURAL 7)
   group.MapPut("{id}", async ...).AddEndpointFilter<IdempotencyEndpointFilter>();
   ```
4. **Endpoint handler içinde SADECE Mediator.Send çağır** ve ApiResponse'u uygun şekilde Results.Json ile dön:
   ```csharp
   group.MapPost("", async (CreateProductRequestDto dto, ISender sender, HttpContext ctx) =>
   {
       var response = await sender.Send(new CreateProductCommand(dto.Name, dto.Price));
       return Results.Json(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }, statusCode: response.StatusCode);
   });
   ```
5. **Authorization:** Group seviyesinde `.RequireAuthorization()` veya tek endpoint'te `.AllowAnonymous()`
6. **MapEndpoints() çağrısı Program.cs'de OTOMATİK** reflection ile tüm IEndpoint sınıflarını tarar, ELLE endpoint ekleme Map* yapma

### Program.cs Kuralı
✅ **Program.cs TOPLAM 25 SATIRI GEÇMEMELİDİR!** (Şu an 22 satır)
- BÜTÜN servis registration → `builder.ConfigureServices()` extension method
- BÜTÜN middleware pipeline → `app.ConfigurePipeline()` extension method
- Program.cs içinde MapPost/MapGet GET/YAZMA!! (Endpoint sınıflarında yapılır

### Anti-Pattern (YAPMA)
❌ Controller tabanlı Web API kullanmak (MVC Controller) - Minimal API + IEndpoint şablon standardı
❌ Program.cs içinde service registration veya endpoint map yazmak - Okunmaz, şişer
❌ Endpoint handler'ında uzun iş kodu yazmak - Hepsi Handler'a gitsin (Mediator pattern)
❌ Idempotency filter'ı POST/PUT'dan unutmak (KURAL 7 cezasını uygula)
❌ Results.Ok(duzDto) kullanmak, ApiResponse ile sarmalanmamış içerik DÖNME

---

## KURAL 11: SECURITY (GÜVENLİK) KURALLARI

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

## KURAL 12: YENİ PACKET / DEPENDENCY EKLEME KURALLARI

Bir NuGet paketi eklemeden ÖNCE:
1. Hangi katmana ait olduğunu belirle:
   - **Domain:** SADECE interface için gerekli paketler (örn: EF Core interface için - kabul)
   - **Application:** Pattern paketleri (MediatR, FluentValidation, AutoMapper)
   - **Infrastructure:** Implementation paketleri (EF Core Provider, BCrypt, MailKit, JWT, Serilog
   - **Api:** Presentation concern paketleri (JwtBearer, Swagger, Serilog.AspNetCore, Scalar.AspNetCore)
2. Paketin lisans kontrolü yap: MIT / Apache 2.0 iyi, GPL/AGPL ticari projelerde riskli (örn: MailKit 4.x GPL, ticari lisans gerekir dikkat et!)
3. Paketin sürümü .NET 11 ile uyumlu mu? (preview olunca dikkat et!)
4. Projeye özgü bir paket mi (template olması için çok spesifik paketleri ekleme ❌).
5. **ApiResponse/PagedResponse/PagedRequest:** Bunları **AYRI bir NuGet paketindeki paket olarak yükleme** — Şablon doğası gereği Application.DTOs.Common içinde doğmalı.

---

## KURAL 13: NAMING CONVENTIONS (İSİMLENDİRME STANDARDI)

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
| Endpoint Filter | [Feature] + EndpointFilter | IdempotencyEndpointFilter |

---

## KURAL 14: RBAC (ROL & YETKİ) KURALLARI (YENİ - ZORUNLU)

Rol ve Yetki sistemi TÜM endpointler için kullanılmalıdır. SuperAdmin/Admin/Manager/Support/User/Customer rolleri ve 50+ Permission (12 grup) mevcuttur.

### Zorunlu
1. **Admin yetkisi gerektiren endpointler için:** `.RequireAuthorization(policy => policy.RequireRole("SuperAdmin,Admin"))` veya daha dar yetki için `.RequireAuthorization(policy => policy.RequireRole("SuperAdmin"))` — HİÇBİR zaman yönetici paneli endpointlerini `.AllowAnonymous()` yapma!
2. **Kullanıcı kendi verisi için:** Handler içinde `currentUserId` ile kaydın `CreatedBy` veya `UserId` alanını KONTROL ET — başka kullanıcının kaydını güncellemesini/silmesini engelle.
3. **Rol atama/revoke işlemleri:** SADECE SuperAdmin/Admin yapabilir. Normal User kendi rolünü değiştiremez.
4. **Permission (İnce yetki) gerekiyorsa:** Rol bazlı yetki yetmiyorsa `.RequireAuthorization("Permission:User.Create")` Policy kullan — Policy DependencyInjection içinde tanımlı olmalı.
5. **Seed SuperAdmin hesabını PRODUCTION'da MUTLAKA DEĞİŞTİR:** varsayılan `superadmin@netcoretemplate.com / Qwerty123!` açık hesap bırakma (Güvenlik açığı).

### Rol Hiyerarşisi (Geniş→Dar)
SuperAdmin → Admin → Manager → Support → User → Customer

### Anti-Pattern
❌ "Herkes Admin" yapma — Minimum yetki prensibi (Least Privilege)
❌ Yeni bir Admin endpointi eklediğinde rol kontrolü eklemek unutulursa **açık** olur!
❌ Kullanıcı ID'sini Client'tan (query/body'den) al — HER ZAMAN `HttpContext.User.Claims` NameIdentifier claim'inden AL (dolandırıcılık engeli).

---

## KURAL 15: RATE LIMITING + LOCKOUT (BRUTE FORCE KORUMASI) (YENİ - ZORUNLU)

### Rate Limiting Politikaları (3 adet)
| Politika Adı | Limit | Süre | Kullanım Yeri |
|---|---|---|---|
| `AuthFixedWindow` | 10 istek | 10 dakika | Login, Register, Forgot-Password, Reset-Password (brute force koruması) |
| `PerUserSlidingWindow` | 150 istek | 60 saniye | Giriş yapmış kullanıcıların tümü (DoS koruması) |
| `GlobalIPFixedWindow` | 500 istek | 15 dakika | Tüm endpointler (IP bazlı genel koruma) |

### Zorunlu
1. **Auth endpoint'leri için:** `.RequireRateLimiting("AuthFixedWindow")` EKLE (Login, Register, ForgotPassword, ResetPassword)
2. **Hesap Kilitleme (Lockout) — 3 hatalı login:** AppUser içinde `AccessFailedCount` ve `LockoutEnd` kullanılır:
   - appsettings `LockoutSettings:MaxFailedAttempts = 5` (3 veya 5 önerilir)
   - `LockoutSettings:LockoutDurationMinutes = 15` (15dk kilit)
   - 6. hatalı denemede kullanıcı 15dk giriş YAPAMAZ — 401 "Hesap kilitlendi X kadar süre sonra tekrar deneyin"
3. **CORS Politikası (SecureCorsPolicy):**
   - Development: `AllowAllOrigins` sadece dev'de
   - Production: `AllowedOrigins` whitelist'den gelenlere izin VER (`appsettings.json:CorsSettings:AllowedOrigins` dizisi)
   - Production'da AllowCredentials + AllowedHeaders: Authorization, Content-Type, X-Idempotency-Key
4. **429 TooManyRequests yanıtı:** Middleware otomatik ApiResponse formatına çevirir, ELLE custom 429 yazma.

### Anti-Pattern
❌ Production'da CORS `AllowAllOrigins` yapmak → CSRF/XSS açığı
❌ Login endpoint'inde RateLimiting atlamak → Brute force şifre kırma saldırısı açık
❌ Lockout mekanizmasını kaldırmak → MaxFailedAttempts = 999999 yapmak

---

## KURAL 16: EMAİL GÖNDERİMLERİ - HANGFIRE KUYRUK (ZORUNLU)

⚠️ **HİÇBİR ZAMAN HTTP Request pipeline'ında `await _emailService.SendAsync(...)` YAPMA!** → Kullanıcı 5-10 sn SMTP cevabını bekler, UI donar, timeout olur.

### Doğru Pattern (Hangfire Background Job)
1. **Handler içinde 2 adım:**
   ```csharp
   // 1. Job Log kaydı (Pending durumda)
   var jobLog = BackgroundJobLog.Create("WelcomeEmail", user.Id, JsonSerializer.Serialize(new { email = user.Email }));
   await _uow.BackgroundJobLogs.AddAsync(jobLog, ct);
   await _uow.SaveChangesAsync(ct);
   // 2. HANGFIRE KUYRUK - AWAIT YOK! Fire-and-Forget
   Hangfire.BackgroundJob.Enqueue<EmailSenderJob>(job => job.Execute(jobLog.Id, user.Email, emailSubject, emailBody, ct));
   ```
2. **EmailSenderJob içinde Polly Retry (3 kez exponential backoff):** 1. deneme 2sn, 2. 4sn, 3. 8sn bekle → SmtpException veya IOException'da tekrar dene.
3. **Başarısız olursa:** JobStatus=Failed, BackgroundJobLog RetryCount artışı + ErrorMessage kaydı. Kullanıcıya HTTP cevabı zaten dönmüştür, kullanıcıyı BEKLEME.

### Zorunlu
1. **3 email türü:** Welcome (kayıt sonrası), ResetPassword (şifre sıfırlama linki), 2FA Code — HEPİ Hangfire kuyruğundan.
2. **NightlyCleanupJob (Her gün 02:00):** 30 gün eski SystemLog + RefreshToken + 365 gün eski AuditEntry sil + LockoutEnd süresi dolmuş kullanıcıların kilidini aç.
3. **Hangfire Dashboard:** `/hangfire` adresi SADECE SuperAdmin ve Admin rolündekiler tarafından görülebilir (Dashboard Authorization filter kullanılır).
4. **Hangfire SQL Storage:** Schema adı `Hangfire` (appsettings'ten değiştirilebilir), ayrı tablolar, AppDbContext tabloları ile karışmaz.

### Anti-Pattern
❌ `await _emailService.SendAsync(...)` HTTP request içinde (Kullanıcı bekler!) ❌
❌ Hangfire Dashboard authsuz → Herkes job'ları görebilir/silebilir/tekrar çalıştırabilir
❌ Polly Retry mekanizmasını kaldırmak → SMTP 503 hatasında mail hiç gitmez

---

## KURAL 17: AUDIT ENTRY - KOLON BAZLI SORGULANABİLİR LOGLAMA (YENİ)

Eski `AppUserActivited` JSON özet logunun YANINDA, **kolon bazlı** `AuditEntry` tablosu kullanılır. Hangi entity'nin hangi property'si ne değişti → TEK SATIR, SORGULANABİLİR (SQL WHERE ile aranır).

### AuditEntry Şeması
| Kolon | Açıklama |
|---|---|
| EntityName | "AppUser", "Product" vb. |
| EntityId | Değişen kaydın Guid Id'si |
| PropertyName | "FirstName", "Email", "Status" |
| OldValue | Değişmeden önceki değer (string) |
| NewValue | Değiştikten sonraki değer |
| ChangedByUserId | İşlemi yapan kullanıcı ID (IHttpContextAccessor'dan) |
| ChangedAt | DateTimeOffset |

### Zorunlu
1. **SaveChanges'ta ProcessAudit:** `AppDbContext.ProcessAudit()` ChangeTracker.Entries'te Modified/Added/Deleted olan HER PROPERTY için ayrı AuditEntry satırı ekler — Otomatik çalışır, ELLE AuditEntry yazma!
2. **Infinite Loop Koruması:** `private bool _auditProcessed` flag + `if (type == typeof(AuditEntry)) continue;` → AuditEntry kendini audit etmez (StackOverflow engeli).
3. **CreatedBy/ModifiedBy/DeletedBy OTOMATİK:** AppDbContext constructor IHttpContextAccessor inject → currentUserId = `NameIdentifier` claim'den alınır → TÜM entitylerin CreatedBy/ModifiedBy/DeletedBy kolonları SET EDİLİR, elle null bırakılamaz.
4. **Audit Sorgusu Örnek:** `GET /api/v1/odata/AuditEntries?$filter=EntityName eq 'AppUser' and PropertyName eq 'Email'&$orderby=ChangedAt desc` → Kimin email'ini değiştirdiğimizi görünür.

### Anti-Pattern
❌ AppUserActivited JSON logunu parse ederek kolon ara → Yavaş, zordur, indexlenmez. Yerine AuditEntry tablosu kullan!
❌ CreatedBy kolonunu null bırakmak → HttpContextAccessor inject UNUTULMUŞ demektir (Data sorumluluğu kimin?)
❌ AuditEntry için `IgnoreQueryFilters()` kullanmamak → 30 gün sonra silinmişse görünmez ama backup'ta durur.

---

## KURAL 18: ODATA v4 DESTEĞİ (YENİ - LİSTE ENDPOİNTLERİ İÇİN ZORUNLU)

Custom `Where/OrderBy/Select` DTO extension metodları YAZMA! Bunun yerine **OData v4** query option'ları kullan: `$filter, $select, $orderby, $expand, $count, $top, $skip`.

### OData Endpoint URL Formatı
```
GET /api/v{version}/odata/{EntitySet}?$filter=...&$select=...&$orderby=...&$count=true&$top=20&$skip=0
```

### Mevcut 7 EntitySet (OData EDM Model)
Users, UserProfiles, Roles, Permissions, SystemLogs, UserActivities, AuditEntries, Notifications (ODataModelBuilder.cs içinde tanımlı)

### Zorunlu
1. **Yeni entity eklediğinde ODataModelBuilder.cs güncelle:**
   ```csharp
   builder.EntitySet<Product>("Products").EntityType.HasKey(p => p.Id);
   // Eğer ilişki expand edilecekse:
   builder.EntitySet<Product>("Products").EntityType.Expand(10).Select().OrderBy().Filter().Count().Page(100, 1);
   ```
2. **OData endpointi:** `app.MapODataRoute("odata", "api/v{version:apiVersion}/odata", GetEdmModel());` — ApiVersioning ile uyumlu v1/v2.
3. **MaxTop sınırı:** appsettings `OData:MaxTop = 100` → Tek istekte en fazla 100 satır (DoS koruması).
4. **$count=true query param varsa:** Response içinde `@odata.count = totalCount` döner, client grid toplam sayısını bilir.
5. **HasApiVersion(1.0, 2.0):** Tüm OData EntitySet'ler hem v1 hem v2 versiyonda yayınlanır — Versiyonlama ile uyumlu.

### Anti-Pattern
❌ Custom `GetProductsFiltered(string searchTerm, string sortBy, int page, ...)` metodu yazıp 20 parametre alan endpoint → KOD KİRLİLİĞİ, OData kullan!
❌ OData'da `$expand` kısıtlamasız yap → 10 seviye nested include EF patlatır (Default MaxExpansionDepth = 2)
❌ ApiVersioning olmadan OData kullan → v1 Users ile v2 Users farklı şema olursa client kırılır.

---

## KURAL 19: SIGNALR GERÇEK ZAMANLI BİLDİRİM (YENİ)

Kullanıcıya anında bildirim göndermek için (Browser Push benzeri) **INotificationService + SignalR Hub** kullan — Doğrudan DB AppNotification yazma!

### Mimarisi
1. **INotificationService interface (Domain katmanı):** `SendToUserAsync(userId, title, message, type)` + `SendToAllAsync(...)`
2. **DatabaseNotificationService (Infrastructure impl):** Önce DB `AppNotification` kaydı INSERT → Sonra SignalR Hub üzerinden client'a WS ile gönder (Çevrimdışı kullanıcı için DB'de kayıt durur, sonra login olunca çeker).
3. **NotificationHub:** `/hubs/notification` endpointi, OnConnectedAsync'de kullanıcıyı `Groups.AddToGroup("user-{userId}")` gruba ekler.
4. **AppNotification tablosu:** UserId, Title, Message, NotificationType (Info/Success/Warning/Error), IsRead, CreatedAt + UserId+IsRead Index.

### Zorunlu
1. **Client notification çekme:** `GET /api/v1/notifications/mine?page=1&pageSize=20` + `PUT /api/v1/notifications/{id}/mark-as-read` — Çevrimdışı sırasında kaybolan bildirimleri çekme.
2. **UserActivity Log:** Her NotificationSend → AppUserActivited tablosunda `NotificationSent` tipi log kaydı (IUserActivityLogger).
3. **Çoklu instance (pod) deployment için:** İLERDE SignalR Redis Backplane ekle (Şu an InMemory — Tek instance yeterli).

### Anti-Pattern
❌ Doğrudan `NotificationHub.Clients.Group(...).SendAsync` çağırıp DB'ye kaydetmemek → Kullanıcı offline'sa bildirim KAYBOLUR.
❌ INotificationService kullanmadan Endpoint handler içinde SignalR Hub çağırmak → Interface segregation ihlali, test edilemez.
❌ Bildirimleri kalıcı silmek yerine IsRead=true yap — Kullanıcı geçmişe baktığında görsün.

---

## KURAL 20: FILE STORAGE - STRATEGY PATTERN 2 İMPL (YENİ)

Dosya upload (avatar, ürün fotoğrafı, belge) için **IFileStorageService interface** kullan; LocalFileStorage (Development wwwroot/uploads) veya AzureBlobStorageService (Production) DI ile seçilir — Kodda Storage kodunu doğrudan yazma!

### Interface (Domain)
```csharp
public interface IFileStorageService
{
    Task<string> UploadFileAsync(string container, string fileName, Stream fileStream, string contentType, CancellationToken ct);
    Task DeleteFileAsync(string container, string fileUrl, CancellationToken ct);
    Task<string?> GetPublicUrlAsync(string container, string fileUrl, CancellationToken ct);
}
```

### Zorunlu
1. **Container Whitelist:** `["avatars", "products", "documents"]` dışında klasör YASAK (appsettings FileStorage:AllowedContainers).
2. **Path Traversal Koruması:** `fileName = Path.GetFileName(fileName)` + `container` whitelist kontrol — `../` ile üst klasöre çıkma engeli.
3. **Extension Whitelist:** `.jpg, .jpeg, .png, .gif, .webp, .pdf` sadece izin verilenler, `AllowedFileExtensions` kontrol + MIME type contentType doğrulaması.
4. **Dosya Boyutu Sınırı:** `MaxFileSizeBytes = 10 * 1024 * 1024` (10 MB) — Request middleware veya Validator ile kontrol.
5. **Storage Provider Seçimi:** appsettings `FileStorage:Provider = "Local"` veya `"AzureBlob"` — DI AddInfrastructure içinde switch-case ile hangi impl kullanılacağı belirlenir.
6. **AzureBlob için ConnectionString:** `FileStorage:AzureBlob:ConnectionString` + `ContainerName` — Production User Secrets / Key Vault.

### Anti-Pattern
❌ LocalFileStorage'da `Path.Combine(wwwroot, userProvidedPath)` yapıp Path.GetFileName kullanmamak → Path traversal saldırısı (sunucuya webshell yükleme!) ❌
❌ Dosya yüklemeden önce extension+boyut kontrolü YAPMAK → zararlı .exe/.php/.aspx sunucuya yüklenir ❌
❌ Storage seçimini kodda `if(isProduction)` ile hardcode yapma → appsettings'ten Provider oku, DI ile bağla

---

## 🚫 KESİNLİKLE YAPILMAMASI GEREKENLER (ANTI-PATTERN BLACKLIST)

1. ❌ **Domain projesine ProjectReference eklemek** (Domain EN ÜST, hiç kimseyi referans almaz!)
2. ❌ **Application Handler içinde doğrudan AppDbContext kullanmak** (IUnitOfWork KULLANILACAK)
3. ❌ **Entity property'lerini public set yapmak** (private/protected set ZORUNLU)
4. ❌ **Yeni entity için özel repository yazmadan IRepository<T> kullanmak** (ÖZEL REPO ZORUNLU)
5. ❌ **Program.cs'i şişirmek** (25 satırdan yukarı çıkarsa ayıp!)
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
19. ❌ **Yeni feature için 7 adım Yeni Entity Ekleme Rehberi (bu dosyanın "EK BÖLÜM 4"ü)'nü atlamak** (her adımı izle)
20. ❌ **MailKit GPL lisansını unutmak** (Production'da MIT lisanslı alternatif kullanma gerekebilir - EK BÖLÜM 3'te not edildi)
21. ❌ **ApiResponse standardını bozmak** — TÜM yanıtlar (başarı + hata) AYNI JSON formatında OLMAK ZORUNDA
22. ❌ **Liste endpointlerinde sayfalama yapmadan 1000+ satırı tek yanıtta döndürmek** (performans sorunu)
23. ❌ **POST/PUT endpointlerinde IdempotencyEndpointFilter eklememek** (double-click ile mükerrer kayıt yaratılır)
24. ❌ **(A24) EmailService.SendAsync'i HTTP Request içinde await ile çağırmak** → Kullanıcı SMTP cevabını 5-10sn bekler. YERİNE Hangfire BackgroundJob.Enqueue KULLAN (KURAL 16)
25. ❌ **(A25) CreatedBy/ModifiedBy kolonlarını null bırakmak** → AppDbContext constructor'a IHttpContextAccessor inject + ProcessAudit içinde NameIdentifier claim'den currentUserId almayı UNUTMA (KURAL 17.3)
26. ❌ **(A26) SecurityStamp Middleware'ı pipeline'dan kaldırmak / atlamak** → ChangePassword veya LogoutAll sonrası ESKI JWT ile hala giriş olur. ZORUNLU (KURAL 11 + SecurityStampMiddleware)
27. ❌ **(A27) Production ortamında CORS AllowAllOrigins yapmak** → CSRF/XSS açığı! SecureCorsPolicy whitelist kullan (KURAL 15)
28. ❌ **(A28) OData v4 yerine custom GetFiltered 20+ parametreli endpoint yazmak** → $filter/$select/$orderby/$count/$top/$skip OData standard kullan (KURAL 18)
29. ❌ **(A29) SaveChanges ProcessAudit'de AuditEntry tablosunu atlamak / kolon bazlı log yapmamak** → AppUserActivited JSON logu parse edilemez, indexlenmez. AuditEntry ZORUNLU (KURAL 17)
30. ❌ **(A30) StrongPasswordValidator'da ÖZEL KARAKTER zorunluluğunu kaldırmak** → 4 şart: BÜYÜK + küçük + sayı + ÖZEL KARAKTER (!@#$%^&*) hepsi ZORUNLU (KURAL 5 + StrongPasswordValidator base)
31. ❌ **(A31) /hangfire Dashboard'ı Authorize olmadan açmak** → Herkes background job'ları görebilir, silebilir, tekrar çalıştırabilir. SuperAdmin/Admin rol filtresi ZORUNLU (KURAL 16)
32. ❌ **(A32) LocalFileStorage'da Path.GetFileName kullanmadan user-provided path ile kaydetmek** → Path traversal saldırısı! `../../Windows/System32/cmd.aspx` gibi webshell yükleme açığı (KURAL 20)
33. ❌ **(A33) JWT Token'a security_stamp claim eklememek** → LogoutAllDevices veya ChangePassword sonrası SecurityStampMiddleware eski JWT'yi reddedemez, kullanıcı çıkış yapmaz (KURAL 11 + SecurityStampMiddleware)
34. ❌ **(A34) Soft Delete geri alma (Restore) işleminde IgnoreQueryFilters KULLANMAMAK** → Status=Deleted kayıtlar Global Query Filter ile GİZLENDİĞİNDEN bulunamaz ve restore edilemez. GetDeleted + Restore'de `.IgnoreQueryFilters()` ZORUNLU (KURAL 11 SoftDelete + BaseEntity.Restore)
35. ❌ **(A35) SaveChanges ProcessAudit içinde AuditEntry entity'sini audit etmeye çalışmak + _auditProcessed flag unutmak** → AuditEntry INSERT → SaveChanges → tekrar ProcessAudit → tekrar AuditEntry INSERT → INFINITE LOOP StackOverflow! `if (type == typeof(AuditEntry)) continue;` + `_auditProcessed = true` flag ZORUNLU (KURAL 17.2)

---

## ✅ YAPILMASI GEREKENLER / CHECKLIST (Yeni Feature Eklerken - 37 Madde)

Önce BU checklist'in TÜMÜNÜ geç, sonra commit et. **0 HATA + 0 CS Uyarı (build-level) + GetDiagnostics 0 ZORUNLUDUR.**

---
### 🏗️ MİMARİ + STANDARTLAR (1-9)
- [ ] **1.** Katman referansları doğru mu? (Api → Infrastructure → Application → Domain, DÖNGÜ YOK, Domain 0 ProjectRef)
- [ ] **2.** Yeni entity BaseEntity'den kalıtım alıyor mu? Property setter private/protected? Non-nullable ctor'da set?
- [ ] **3.** Entity için ÖZEL IEntityRepository + Impl var mı? IUnitOfWork'da çoğul property (Products) var mı? (KURAL 3)
- [ ] **4.** EF Configuration (IEntityTypeConfiguration) yazıldı mı? PK/FK/Index/MaxLength tanımlandı mı?
- [ ] **5.** Feature/[X] için Command/Query **ApiResponse<T> veya PagedResponse<T>** DÖNÜYOR MU? (SADECE düz DTO YOK! KURAL 4/5)
- [ ] **6.** Handler'daki BÜTÜN return'lar **ApiResponse.Success/Paged(...)** ile SARILDI MI? (KURAL 5)
- [ ] **7.** TÜM Command'lar için Validator yazıldı mı? PagedRequest varsa `PageNumber >=1 && PageSize <=200` doğrulaması var mı?
- [ ] **8.** DTO'lar (record) ve MappingProfile CreateMap<,> güncellendi mi?
- [ ] **9.** Build: `dotnet build NetCoreTemplate.slnx -v q` 0 HATA + 0 CS-LEVEL UYARI (sadece NuGet transit uyarıları kabul edilir) + `GetDiagnostics` 0

---
### 🔗 ENDPOINT + API STANDARTLARI (10-15)
- [ ] **10.** Endpoints/[X]Endpoints.cs sınıfı IEndpoint implement mi? Program.cs'e ELLE Map* YAZILMADI MI? (KURAL 10)
- [ ] **11.** BÜTÜN POST/PUT/DELETE endpointlerine `.AddEndpointFilter<IdempotencyEndpointFilter>()` EKLENDİ Mİ? (KURAL 7)
- [ ] **12.** TÜM GET liste endpointleri SAYFALI mı? `ApiResponse.Paged(items, page, size, totalCount)` KULLANILDI MI? (KURAL 6)
- [ ] **13.** BÜTÜN yanıtlar `Results.Json(response, JsonOptions, statusCode: response.StatusCode)` pattern'ı ile mi? `Results.Ok(duzDto)` YOK! (KURAL 5)
- [ ] **14.** API Versiyonlama: Endpoint grubu `.HasApiVersion(1.0)` + varsa v2 için `.HasApiVersion(2.0)` eklendi mi? (KURAL 10)
- [ ] **15.** Auth endpointleri (Login/Register/Forgot/Reset) `.RequireRateLimiting("AuthFixedWindow")` + RateLimit config doğru mu? (KURAL 15)

---
### 🛡️ RBAC + GÜVENLİK (16-22)
- [ ] **16.** Admin/Manager endpointleri `.RequireAuthorization(p => p.RequireRole("SuperAdmin,Admin"))` ile KISITLANDI MI? (KURAL 14)
- [ ] **17.** SuperAdmin seed hesabı PRODUCTION'da değiştirildi mi? (Email/şifre - Güvenlik açığı KURAL 14.5)
- [ ] **18.** CORS SecureCorsPolicy: Production AllowedOrigins whitelist'ten mi? AllowCredentials + ExposedHeaders doğru mu? (KURAL 15)
- [ ] **19.** Lockout (Hatalı Login): MaxFailedAttempts=5, LockoutDurationMinutes=15, Login handler AccessFailedCount/LockoutEnd set ediyor mu? (KURAL 15)
- [ ] **20.** Strong Şifre Policy: Register/Reset/ChangePassword validator'ları StrongPasswordValidator base KALITIMI alıyor, 4 ŞART (Büyük+Küçük+Sayı+Özel) ZORUNLU mu? (KURAL 5/30)
- [ ] **21.** JWT security_stamp claim + SecurityStampMiddleware (30sn cache): ChangePassword/LogoutAll sonra ESKI JWT 401 alıyor mu? (KURAL 11/33)
- [ ] **22.** CreatedBy/ModifiedBy/DeletedBy: AppDbContext constructor IHttpContextAccessor inject + ProcessAudit currentUserId claim SET EDİYOR MU? (KURAL 17.3 / A25)

---
### 🗄️ AUDIT + LOG + RESTORE (23-26)
- [ ] **23.** AuditEntry (Kolon bazlı): SaveChanges'da HER Modified entity PROPERTY başına TEK AuditEntry SATIRI ekleniyor mu? (EntityName/EntityId/PropertyName/Old/New - KURAL 17)
- [ ] **24.** AuditEntry Infinite Loop Koruması: `_auditProcessed` flag + `if (type == typeof(AuditEntry)) continue;` VAR MI? (KURAL 17.2 / A35)
- [ ] **25.** Soft Delete Restore: GetDeletedEntities + RestoreDeletedEntity `.IgnoreQueryFilters()` KULLANIYOR MU? Restore Status=Deleted → Status=Active + Entity.Restore() domain metodu? (KURAL 11 / A34)
- [ ] **26.** SaveChanges sonrası AppUserActivited (JSON özet) + AuditEntry (kolon bazlı) 2 tabloya da kayıt DÜŞÜYOR MU? Middleware Exception SystemLog tablosuna kaydediyor mu?

---
### ⏱️ HANGFIRE + EMAİL KUYRUK (27-29)
- [ ] **27.** 3 Auth Email (Welcome/ResetPassword/2FA): Handler içinde `await _emailService.SendAsync` YOK! Yerine `BackgroundJob.Enqueue<EmailSenderJob>(...)` FIRE-AND-FORGET kullanıldı mı? (KURAL 16 / A24)
- [ ] **28.** EmailSenderJob: Polly 3 Retry exponential backoff (2sn/4sn/8sn) SmtpException/IOException'da çalışıyor mu? BackgroundJobLog Status Pending→Processing→Succeeded/Failed set ediliyor mu?
- [ ] **29.** Hangfire Dashboard `/hangfire`: DashboardAuthorization filter ile SADECE SuperAdmin/Admin erişebiliyor mu? NightlyCleanupJob (Her gün 02:00) RecurringJob olarak eklendi mi? (KURAL 16 / A31)

---
### 🗂️ ODATA + FILE STORAGE + SIGNALR + HEALTH + OTEL (30-37)
- [ ] **30.** OData v4 Desteği: ODataModelBuilder.cs'de yeni entity için EntitySet + HasKey tanımlandı mı? `app.MapODataRoute("odata", "api/v{version:apiVersion}/odata", ...)` endpointi var mı? (KURAL 18 / A28)
- [ ] **31.** OData Sınır: MaxTop=100, $count=true, HasApiVersion(1.0/2.0), $expand MaxExpansionDepth=2 ayarları doğru mu?
- [ ] **32.** File Storage: IFileStorageService interface (Upload/Delete/GetPublicUrl) kullanılıyor mu? Local/Azure impl DI ile seçiliyor, Container Whitelist + Path.GetFileName (Path Traversal Koruması) var mı? (KURAL 20 / A32)
- [ ] **33.** File Storage: Extension Whitelist + MaxFileSizeBytes=10MB Validator kontrolü yapılıyor mu? AzureBlob ConnectionString Key Vault/User Secrets'te saklanıyor mu?
- [ ] **34.** SignalR Bildirim: INotificationService + DatabaseNotificationService (Önce DB AppNotification INSERT → Sonra Hub Group Send) kullanılıyor mu? NotificationHub `/hubs/notification` user-{userId} grubuna ekliyor mu? (KURAL 19)
- [ ] **35.** Health Check: 4 HealthCheck (Database/Smtp/FileStorage/Hangfire) + /health/live, /health/ready, /health/detailed, /health/metrics endpointleri çalışıyor mu?
- [ ] **36.** OpenTelemetry: AddOpenTelemetry() Tracing (AspNetCore/EF/HttpClient) + Metrics + OTLP/Console/Prometheus exporter'ları configli mi? `/metrics` endpointi Prometheus formatı dönüyor mu?
- [ ] **37.** BU DOSYAYI (AI_RULES_FOR_THIS_REPO.md) güncellemeyi UNUTTUN MU? Yeni KURAL eklemen gerekiyorsa ekle, Anti-pattern'i güncelle.

---

## 📌 Referans Belgeler
- Katmanlı Mimari: https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
- CQRS + MediatR: https://github.com/jbogard/MediatR
- FluentValidation: https://docs.fluentvalidation.net/
- Clean Architecture: https://github.com/jasontaylordev/CleanArchitecture (referans proje)
- Scalar UI: https://scalar.com (modern API dokümantasyonu, `/scalar` adresinde yayınlanır)

---

---

# 🗺️ EK BÖLÜM: Proje Yol Haritası, Entity Ekleme Rehberi ve Teknik Bilgiler

> Aşağıdaki bölüm eski PROJECT_TEMPLATE_ROADMAP.md içeriğidir. TEK KURAL DOSYASI olduğu için BURADA toplandı. Başka yol haritası dosyası yok!

---

## EK BÖLÜM 1: MİMARİ ÖZETİ

```
Presentation (Sunum Katmanı)
└── NetCoreTemplate.Api (Minimal API + IEndpoint Pattern, Program.cs ≤25 satır)
    → Sorumlulukları: HTTP endpoint tanımlama, middleware (Exception+Idempotency), auth, DI, Swagger UI + Scalar UI

Core (Çekirdek İş Katmanı)
├── NetCoreTemplate.Application (İş Kuralları, CQRS)
│   └── DTOs/Common: ApiResponse, ApiResponse<T>, PagedResponse<T>, PagedRequest (ZORUNLU STANDARTLAR)
│   └── Features/[Biri] : IRequest<ApiResponse<T>>, IRequestHandler, FluentValidation Validator'lar
│   └── DTOs: Giriş/çıkış modelleri
│   └── Behaviors: MediatR pipeline (ValidationBehavior → ValidationException, LoggingBehavior)
│   └── Mappings: AutoMapper Profile'lar
│   └── Exceptions: NotFound, Validation, Business, Unauthorized (Middleware ApiResponse.Fail'e çevirir)
│   └── DependencyInjection.cs: AddApplicationServices()
│   Kural: SADECE Domain katmanını REFERANS ALIR

├── NetCoreTemplate.Domain (Varlıklar + Kurallar)
│   └── Entities: AppUser, AppUserProfile, AppUserRefreshToken, AppUserActivited, Setting, SystemLog
│   └── Seedworks/BaseEntity: Id, Created/Modified/DeletedDate, Created/Modified/DeletedBy, Status (soft delete)
│   └── Enums: EntityStatus, UserActivityType, TokenType, TwoFactorType, SystemLogLevel
│   └── Interfaces: IAppDbContext, IUnitOfWork, IRepository<T> + HER ENTİTY İÇİN ÖZEL REPO INTERFACE, IIdempotencyService
│   └── Interfaces/Security: IPasswordHasher, ITokenService, IUserActivityLogger
│   └── Interfaces/Services: IEmailService
│   Kural: HİÇBİR KATMANI REFERANS ALMAZ (EN ÜST)

└── NetCoreTemplate.Infrastructure (Altyapı)
    └── Persistence: AppDbContext (ProcessAudit override - SaveChanges audit+soft delete), Configurations, Repositories, UnitOfWork
    └── Idempotency: MemoryIdempotencyService (IMemoryCache) + IdempotencySettings (120dk)
    └── Security: PasswordHasher (BCrypt WorkFactor=11), JwtTokenService, TokenGenerator, EncryptionHelper (AES-256), HashHelper (SHA/HMAC), UserActivityLogger
    └── Services: EmailService (MailKit SMTP), IdempotencyService
    └── Logging: SerilogConfigurator (Console + MSSqlServer sink (SystemLogs tablosu)
    └── DependencyInjection.cs: AddInfrastructureServices()
    Kural: SADECE Application katmanını REFERANS ALIR
```

### Katman Referans Zinciri
Api → Infrastructure → Application → Domain
❌ Döngüsel referans YOK, ❌ Domain hiç ProjectReference almaz

### Kullanılan Pattern'ler
- **CQRS (MediatR 14.x TEK PAKET):** Her iş → Ayrı Command/Query + Handler + Validator (TEK PAKET — eski Extensions paketi GEREKSİZ)
- **Repository + UnitOfWork:** Her entity için ÖZEL Repository (orn: IAppUserRepository) + Generic base (IRepository<T>) + IUnitOfWork
- **ApiResponse Standardı:** Tek JSON format (KURAL 5)
- **Paged Sayfalama:** PagedRequest + PagedResponse (KURAL 6)
- **Idempotency:** IEndpointFilter + InMemoryCache (KURAL 7)
- **FluentValidation 12.x:** Her Command/Query için Validator sınıfı + ValidationBehavior otomatik çalışır
- **AutoMapper 16.x:** Entity ↔ DTO mapping
- **Serilog 4.x:** Yapılandırılmış loglama + Console + SystemLogs (MSSQL)
- **SaveChanges Audit:** AppDbContext.ProcessAudit() otomatik Created/Modified/Deleted tarih stamp + soft delete + Değişiklikleri AppUserActivited tablosunda loglar

---

## EK BÖLÜM 2: MEVCUT ÖZELLİKLER (TAMAMLANANLAR)
☑️ **Standart Yanıt (ApiResponse + PagedResponse):** Başarı/hata AYNI JSON formatı (KURAL 5)
☑️ **Sayfalama Sistemi:** Bütün liste endpointleri PagedResponse<T> (KURAL 6)
☑️ **Mükerrer İşlem Engel (Idempotency):** X-Idempotency-Key + InMemoryCache (KURAL 7)
☑️ Kullanıcı Yönetimi: AppUser + AppUserProfile (CRUD + Profile
☑️ Authentication & Authorization: JWT Bearer, Access Token + Refresh Token (rotate'lı), 2FA (Email)
☑️ Auth Endpoint'leri: /api/auth/{login, register, refresh, forgot-password, reset-password, enable-2fa, verify-2fa, logout, me} (Hepsi ApiResponse, POST'lar Idempotency filter ile)
☑️ Şifreleme: BCrypt WorkFactor=11, AES-256 Enc/Dec, SHA256/512 + HMAC helper
☑️ Kullanıcı Aktivite Logu: AppUserActivited tablosu (Login, Logout, PasswordChanged, EntityCreated/Updated/Deleted vb.
☑️ Sistem Logu: SystemLog tablosu (Exception Handling Middleware + Serilog MSSqlServer Sink
☑️ Email Servisi: MailKit SMTP (Hoş geldin, şifre sıfırlama, email doğrulama, 2FA kodu)
☑️ Global Exception Handling: **ApiResponse.Fail(...)** JSON cevap + SystemLog tablosuna kayıt (KURAL 5/8)
☑️ Ayarlar: Setting tablosu (Global + User bazlı Key/Value)
☑️ Swagger UI + Scalar UI: `/swagger` ve `/scalar` adreslerinde 2 ayrı modern API dokümantasyon arayüzü

---

## EK BÖLÜM 3: SONRAKİ GELİŞTİRME ADIMLARI (ÖNERİLEN ÖNCELİK)

### 🔴 Yüksek Öncelik (Kullanılmadan önce tamamlanmalı)
1. **EF Core Migration + DB Oluşturma:**
   ```powershell
   dotnet ef migrations add InitialCreate -p Core/NetCoreTemplate.Infrastructure -s Presentation/NetCoreTemplate.Api
   dotnet ef database update -p Core/NetCoreTemplate.Infrastructure -s Presentation/NetCoreTemplate.Api
   ```
2. **Production Güvenlik Ayarları:**
   - `JwtSettings.SecretKey` appsettings'ten silin, User Secrets veya Azure Key Vault kullan
   - Strong password policy uygulayın (şu an 8 karakter + büyük/küçük + sayı
   - CORS politikasını "AllowAll" yerine production'da gerçek domainleri listeleyin
   - HTTPS zorunlu olsun + HSTS header ekle
   - Rate Limiting ekle (Login/Register brute force koruması için)
3. **Rol ve Yetki Sistemi:** `AppRole`, `AppUserRole`, `AppPermission`, `AppRolePermission` entity'leri + [Authorize(Roles="Admin")]
4. **Email Provider Değiştir:** MailKit GPL lisanslı. Production için: SendGrid / Mailgun / FluentEmail (MIT lisanslı) geçin veya System.Net.Mail (eski)
5. **Idempotency'i Redis'e Taşı:** Multi-instance (pod) deployment'ta InMemoryCache instance başına olduğundan, 2. istek farklı pod'a giderse çalışmaz. Production → IDistributedCache + StackExchangeRedis kullan

### 🟡 Orta Öncelik
6. **TOTP 2FA:** Google Authenticator/Microsoft Authenticator desteği (Otp.NET paket
7. **Distributed Cache:** Microsoft.Extensions.Caching.StackExchangeRedis → IDistributedCache (Idempotency+Session)
8. **Background Jobs:** Hangfire veya Quartz.NET → Recurring Job'lar, email kuyruğu
9. **Health Check:** Microsoft.Extensions.Diagnostics.HealthChecks → /health endpoint'i (DB, SMTP, Redis)
10. **API Versioning:** Asp.Versioning.Http
11. **Rate Limiting:** Microsoft.AspNetCore.RateLimiting
12. **Localization:** Resources (.resx) + RequestLocalizationMiddleware (TR/EN)
13. **ApiResponse.Success + Message:** Kullanıcıya daha açıklayıcı çok dilli mesajlar

### 🟢 Düşük Öncelik / Opsiyonel
14. **CQRS Read/Write Ayırma:** Write DB + Read DB (Dapper ile okuma)
15. **SignalR:** Gerçek zamanlı bildirimler
16. **Blob Storage:** Azure Blob / MinIO / AWS S3 (Avatar ve dosya yüklemeleri için)
17. **Audit History:** Tüm entity değişikliklerini ayrı bir AuditLogs tablosunda (zaten SaveChanges override kısmen yapıyor)
18. **Unit & Integration Test:** xUnit + TestContainers (Test DB)
19. **Docker Support:** Dockerfile + docker-compose.yml (API, DB, Redis)
20. **API Gateway / Reverse Proxy: YARP / nginx**
21. **OpenTelemetry:** Metrics + Tracing (Prometheus + Grafana)
22. **Multi Tenant:** Multi-tenant mimari (Client/Şirket ayırıcı)

---

## EK BÖLÜM 4: YENİ ENTİTY EKLEME REHBERİ (11 ADIM)

Yeni bir entity (örn: `Product`, `Category` vb.) eklemek için bu 11 adımı SIRA İLE izleyin. Örnek entity: `Product`. **HİÇBİR ADIMI ATLAMA!**

### Adım 1: Domain Katmanı
1. `Domain/Entities/Product.cs` oluştur
   - `BaseEntity`'den kalıtım al
   - Property setterları **private/protected** yap
   - **Constructor** oluştur ve non-nullable property'leri constructor'da set et
   - Domain metodları ekle (örn: `UpdatePrice()`, `Deactivate()`
2. Gerekliyse `Domain/Enums/` içinde yeni enum ekle (örn: `ProductStatus.cs`)
3. `Domain/Interfaces/Repositories/IProductRepository.cs` oluştur
   - `IRepository<Product>` implement et
   - Özel metodlar ekle (örn: `Task<Product?> GetBySkuAsync(string sku)`)
4. `Domain/Interfaces/IUnitOfWork.cs` içine: `IProductRepository Products { get; }` property'sini ekle
5. (Opsiyonel) Domain event fırlatacaksan Domain Event ekle

### Adım 2: Infrastructure Katmanı - EF Core + Repository + UoW
1. `Infrastructure/Persistence/Configurations/ProductConfiguration.cs` oluştur (IEntityTypeConfiguration<Product>)
   - PK, FK, Unique Index, string MaxLength, ilişkileri tanımla
2. `Infrastructure/Persistence/Repositories/ProductRepository.cs` oluştur
   - `Repository<Product>, IProductRepository` implement
   - Özel metodları yaz (AsNoTracking unutma!)
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

### Adım 3: Application Katmanı - DTO'lar (ApiResponse ile Sarılmış!)
1. `Application/DTOs/Products/ProductDto.cs` (Read DTO record)
2. `Application/DTOs/Products/CreateProductRequestDto.cs` (Create Input)
3. `Application/DTOs/Products/UpdateProductRequestDto.cs` (Update Input)
4. List query için: **PagedProductRequest (PagedRequest kalıtım) + Validator** (PageSize ≤ 200 doğrulaması)

### Adım 4: Application Katmanı - Validator'lar (FluentValidation)
1. **Her Command/Query için Validator YAZ:**
   - `CreateProductCommandValidator.cs`, `UpdateProductCommandValidator.cs`, `GetProductQueryValidator.cs`
   - **Paged validator:** `PageNumber >= 1 && PageSize >= 1 && PageSize <= 200`

### Adım 5: Application Katmanı - Features (CQRS - ApiResponse TÜMÜNDE SAR)
1. Features/Products/Commands/Create/
   - CreateProductCommand.cs → `IRequest<ApiResponse<ProductDto>>` (ZORUNLU KURAL 4.1 ❗)
   - CreateProductCommandHandler.cs → Başarıyı `ApiResponse.Success(dto, 201Created)` ile SAR
   - CreateProductCommandValidator.cs
2. Features/Products/Commands/Update/ → UpdateProductCommand + Handler + Validator
3. Features/Products/Commands/Delete/ → DeleteProductCommand + Handler (NoContent → ApiResponse.Success(Unit.Value, 204))
4. Features/Products/Queries/GetById/ → GetProductByIdQuery + Handler (ApiResponse<ProductDto>)
5. **Features/Products/Queries/GetAll/ → GetProductsQuery (PagedRequest kalıtım) + Handler: `ApiResponse.Paged(items, page, size, totalCount)` (ZORUNLU KURAL 6 ❗)**
6. `Application/Mappings/MappingProfile.cs` → Product ↔ Product DTO mapping EKLE

### Adım 6: Presentation (API) Katmanı - Endpoints (IEndpoint + Idempotency + ApiResponse Results.Json)
1. `Api/Endpoints/ProductsEndpoints.cs` oluştur, `IEndpoint` implement et
2. `Map()` metodu içinde:
   ```csharp
   var group = app.MapGroup("api/products").WithTags("Products").RequireAuthorization();
   group.MapGet("{id}", async (Guid id, ISender s) =>
   {
       var r = await s.Send(new GetProductByIdQuery(id));
       return Results.Json(r, JsonOptions, statusCode: r.StatusCode);
   });
   // GET ALL (SAYFALI)
   group.MapGet("", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender s) =>
   {
       var r = await s.Send(new GetAllProductsQuery(pageNumber, pageSize));
       return Results.Json(r, JsonOptions, statusCode: r.StatusCode);
   });
   // POST/PUT/DELETE: IDEMPOTENCY FILTER (ZORUNLU KURAL 7 ❗)
   group.MapPost("", async (...) => {...}).AddEndpointFilter<IdempotencyEndpointFilter>();
   group.MapPut("{id}", async (...) => {...}).AddEndpointFilter<IdempotencyEndpointFilter>();
   group.MapDelete("{id}", async (...) => {...}).AddEndpointFilter<IdempotencyEndpointFilter>();
   ```
3. (Otomasyon: MapEndpoints() reflection ile otomatik olarak ekler, ekstra bir şey yapmana gerek YOK

### Adım 7: AppSettings + Dependency Güncellemeleri
1. Idempotency için yeni bir ayara gerek yok (default 120dk)
2. Eğer yeni bir Infrastructure servis eklediysen (IProductCache vb.), DI registration unutma

### Adım 8: Test + Dokümantasyon (ZORUNLU)
1. Build + `dotnet build NetCoreTemplate.slnx` 0 hata (0 uyarı ideal)
2. **BU DOSYAYI (AI_RULES_FOR_THIS_REPO.md) güncelle (yeni entity ile ilgili checklist notları eklenebilir**
3. Scalar UI (`/scalar`) + Swagger UI (`/swagger`) test et
   - POST ürün oluştururken **2 kez farklı istek, AYNI X-Idempotency-Key** → 2. yanıt AYNI, 1. yanıtın aynısı mı? (KURAL 7 doğrulaması)
   - Liste endpointi: pageNumber=1, pageSize=2 → 2 kayıt + totalCount doğru mu? (KURAL 6 doğrulaması)
4. Veritabanında AppUserActivited tablosunda EntityCreated/Updated/Deleted loglarının otomatik düştüğünü doğrula (KURAL 8)
5. ProductNotFoundException fırlat → StatusCode=404 + ApiResponse.IsSuccess=false + Message=Doğru format? (KURAL 5 doğrulaması)

### Adım 9: ODATA v4 ENTİTYSET TANIMLAMA (ZORUNLU - KURAL 18)
1. `Api/OData/ODataModelBuilder.cs` dosyasını aç, yeni entity için EDM modele EKLE:
   ```csharp
   // Product EntitySet + HasKey
   builder.EntitySet<Product>("Products").EntityType.HasKey(p => p.Id);
   // Opsiyonel: Eğer ilişkiler expand edilecekse, yetkiler:
   builder.EntitySet<Product>("Products").EntityType
       .Expand(2)    // Max 2 seviye expand (Product → Category → ProductFeatures - KURAL 18.2)
       .Select()     // $select izni (sadece istediğin kolonları çek
       .OrderBy()    // $orderby izni
       .Filter()     // $filter izni
       .Count()      // $count=true izni (totalCount)
       .Page(100, 1); // MaxTop=100, PageSize sınırı (appsettings OData:MaxTop ile AYNI olsun
   ```
2. Entity'nin API Version ile uyumlu olduğundan emin ol: `builder.EntitySet<Product>("Products").EntityType.HasApiVersion(new ApiVersion(1, 0));` (v1.0)
3. Entity navigation property (örn: Product → Category) varsa EntityType içinde `.Navigation(k => k.Category) ile tanımla ki $expand çalışsın
4. OData smoke test: `GET /api/v1/odata/Products?$filter=Price gt 100&$select=Id,Name&$count=true&$top=2` tarayıcıda/postman test et. Toplam 2 kayıt + @odata.count toplam ürün sayısını doğru vermeli (KURAL 18 doğrulaması)

### Adım 10: SEED DATA (GEREKİYORSA (Opsiyonel)
Eğer yeni entity'nin **sistem çalışır çalışmaz ilk verilerle dolması gerekiyorsa (örn: ProductCategory, Setting tipler, önceden tanımlı statik değerler):
1. `Infrastructure/Persistence/SeedData/AppDbInitializer.cs` içinde YENİ `SeedProductsAsync() fonksiyonu yaz:
   ```csharp
   private static async Task SeedProductsAsync(AppDbContext context, CancellationToken ct)
   {
       // ⚠️ Idempotent: Eğer veri varsa TEKRAR EKLEME!
       if (await context.Products.IgnoreQueryFilters().AnyAsync(ct)) return;
       
       var products = new List<Product>
       {
           Product.Create("Örnek Ürün 1", 99.90m, "Açıklama"),
           Product.Create("Örnek Ürün 2", 149.00m, "Açıklama 2"),
       };
       await context.Products.AddRangeAsync(products, ct);
       await context.SaveChangesAsync(ct);
       _logger.LogInformation("Seed: {Count} Product kaydedildi", products.Count);
   }
   ```
2. `InitializeDatabaseAsync` içinde `await SeedProductsAsync(context, ct);` çağır — çağrı sırası doğru olsun (Foreign key gerekiyorsa sonraya koy).
3. Seed verilerini **DEĞİŞMEZ ise (Statik enum tipler): Seed fonksiyonunun başında `IgnoreQueryFilters().AnyAsync() return ile idempotent yap. 2. ayağa kalkmada duplicate yaratma.
4. EF Core Migration sonrası `dotnet ef database update` sonrası DB'de Products tablosunda 2 satır görünmü kontrol et (Adım 8 smoke test).

### Adım 11: YETKİ + ADMİN ENDPOİNTLERİ ROL KONTROLÜ (ZORUNLU - KURAL 14/16)
Eğer yeni entity ADMIN özelindiyse (ürün/yetki yönetimi, ayarlar,yönetim paneli) AŞAĞIDAKİ 3 KONTROLÜ YAP (Hangfire Dashboard gibi):
1. **Endpoint Rol Kısıtlaması:** ProductsEndpoints.cs MapGroup içinde `.RequireAuthorization(policy => policy.RequireRole("SuperAdmin,Admin"))` — Customer rolü olmayan erişemesin.
2. **Handler Ownership Check:** Eğer kullanıcı (User rolü ise kendi Product kaydını (CreatedBy == currentUserId) güncelleyebilmeli, başka kullanıcının Product'ını GÜNCELLEYEMEMELİ/SILEMEMELİ. Handler içinde kontrol et:
   ```csharp
   if (product.CreatedBy != currentUserId && !isAdmin)
       throw new UnauthorizedException("Bu kaydı değiştirme yetkiniz yok");
   ```
3. **Delete/Restore/Soft Delete:** GetDeleted ve Restore endpointleri SADECE Admin/SuperAdmin yetkilendirilmelidir (Normal user kendi sildiği ürünü göremez, yönetici görebilir/restore edebilir).
4. **Loglama:** Bu 3 adımı (oluşturma, güncelleme, silme) AuditEntry + AppUserActivited 2 tabloya da kayıt düştüğünü Adım 8 testleri içinde ayrıca doğrula.

---

## EK BÖLÜM 5: SIK KULLANILAN KOMUTLAR
```powershell
# Solution restore + build
dotnet restore NetCoreTemplate.slnx
dotnet build NetCoreTemplate.slnx -warnaserror

# Migration
dotnet ef migrations add InitialCreate -p Core/NetCoreTemplate.Infrastructure -s Presentation/NetCoreTemplate.Api
dotnet ef database update -p Core/NetCoreTemplate.Infrastructure -s Presentation/NetCoreTemplate.Api
dotnet ef migrations list -p Core/NetCoreTemplate.Infrastructure -s Presentation/NetCoreTemplate.Api --no-connect

# Run API
cd Presentation/NetCoreTemplate.Api
dotnet run
# Swagger UI: https://localhost:PORT/swagger
# Scalar UI:  https://localhost:PORT/scalar   (MODERN — önerilen)
# OpenAPI JSON: https://localhost:PORT/openapi/v1.json

# User Secrets (Production gizli anahtarlar için — asla appsettings'e yazma!)
dotnet user-secrets init --project Presentation/NetCoreTemplate.Api
dotnet user-secrets set "JwtSettings:SecretKey" "COK_GIZLI_512bit_ANAHTAR_BURAYA"
dotnet user-secrets set "EmailSettings:SmtpPassword" "SMTP_SIFREN"
```

---

## EK BÖLÜM 6: YAYGIN HATALAR ve ÇÖZÜMLERİ
| Hata | Çözüm |
|------|-------|
| Nullable disable warning CS8618: Entity constructor'ında non-nullable property'i set et veya `= null!` kullanma (tercih: constructor) |
| `IUnitOfWork.{Entity} property'ı null dönüyor` | UnitOfWork.cs'de lazy init (private IProductRepository? _products; public IProductRepository Products => _products ??= new ProductRepository(_context);) yazıldığından emin ol |
| SaveChanges infinite loop (StackOverflow) | ProcessAudit()'te `if (entry.Entity is AppUserActivited or SystemLog) continue;` ATLAMA koşulu UNUTULMAZ |
| ApiResponse Success-Fail formatı uyuşmuyor (anonymous tip) | Middleware haricinde ELLE new{...} yazma; handler'larda exception fırlat → middleware çevirir |
| PagedResponse'ta TotalPages hatalı | `(int)Math.Ceiling(totalCount / (double)Math.Max(1, pageSize))` formülü (double cast olmadan integer division olur) |
| Idempotency çalışmıyor (2. istek tekrar DB insert) | (1) Endpointte `.AddEndpointFilter<IdempotencyEndpointFilter>()` var mı? (2) Client X-Idempotency-Key gönderiyor mu? (3) InMemory cache ayakta mı? (production'ta Redis olmalı, instance restart cache'i siler) |
| JWT doğrulama hatası (401) | JwtSettings'teki Issuer, Audience ve SecretKey değerleri → JwtBearer TokenValidationParameters ile AYNI mı? |
| Login başarısız ama görünür hata yok | AppUserActivited tablosunu kontrol et: LoginFailed logu + AccountLocked var mı? LockoutEndDate geçmemiş mi? |
| Swagger Authorize butonu yok | DependencyInjection.cs SwaggerGen'de SecurityDefinition + SecurityRequirement yapılandırmasını kontrol et (artık Swagger 10.x ile doğru çalışmalı |
| Email gönderimi başarısız | SmtpHost/SmtpPort/EnableSsl/User/Pass doğru mu? Local development'ta mailtrap.io veya Papercut kullan (localhost:25 çoğunlukla kapalıdır) |
| "Microsoft.AspNetCore.App FrameworkReference gerekli" hatası Application katmanında | Application katmanında StatusCodes veya IResult kullanma → bunlar API veya Web Framework bağımlılığıdır. Alternatif: int sabit kullan (200, 400) ya da FrameworkReference ekle ( dikkatli Class Library kirlenir — template'de şu an minimal FrameworkReference ile çalışıyor) |

---

**Version**: 3.0 | **Tarih**: 2026-09-30 | **Tek Kural Dosyası**: AI_RULES_FOR_THIS_REPO.md (Başka KURAL MD YOK)
**Son Güncelleme**: v3.0 14 Yeni Özellik (RBAC/RateLimit/Hangfire/AuditEntry/OData/SignalR/FileStorage/OTel/HealthCheck/ApiVersioning/RestoreSoftDelete/Lockout/SecurityStamp) — 7 Yeni KURAL K14-K20, 12 Anti-pattern A24-A35, Checklist 17→37 madde, Entity Rehberi 8→11 adım
