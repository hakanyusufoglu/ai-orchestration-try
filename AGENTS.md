# OrderService

Sipariş yönetimi için .NET 8 Web API. Vertical Slice Architecture, MediatR (CQRS), EF Core + PostgreSQL.
Ayrıntılı kod kalıpları ekip standardı `vertical-slice-dotnet` skill'indedir; bu dosya yalnızca temel kuralları içerir.

## Komutlar

- Build: `dotnet build OrderService.slnx`
- Test: `dotnet test OrderService.slnx`
- Format: `dotnet format OrderService.slnx`

Bir işi bitmiş saymadan önce build ve test çalıştırılmalı ve geçmelidir.

## Yapı

- `src/Shared/` — yalnızca altyapı primitive'leri (Result, AppError, Pagination, Behaviors). Feature veya domain kodu girmez.
- `src/OrderService.Api/Features/{Feature}/Commands|Queries/{UseCase}/` — her use case kendi klasöründe; Command/Query, Handler, Validator ve DTO ayrı dosyalardır.
- `src/OrderService.Api/Features/{Feature}/{Feature}Endpoints.cs` — o feature'ın tüm endpoint kayıtları.
- `src/OrderService.Api/Domain/` — entity'ler (private setter, factory metot, davranış metotları) ve enum'lar.
- `src/OrderService.Api/Infrastructure/Persistence/` — AppDbContext ve entity configuration'ları.
- `tests/OrderService.Tests/Features/{Feature}/` — handler ve validator testleri.
- `docs/decisions/` — mimari kararlar (ADR). Plan yapmadan veya ilgili alanda kod yazmadan önce oku; bir karara aykırı iş gerekiyorsa önce kullanıcıya sor.

## Temel kurallar

- Handler dönüş tipi `Result<T>`; iş hatası exception ile değil `AppError` ile döner.
- Validator yalnızca Command'lere yazılır, Query'lere yazılmaz.
- Query'ler `AsNoTracking()` + `.Select()` projection kullanır; `Include` yalnızca entity'nin kendisi gerekiyorsa.
- Sayfalı sorgularda sıralama benzersiz bir anahtarla biter (ör. `.ThenByDescending(x => x.Id)`); offset `long` olarak hesaplanır.
- Tüm async metotlar `CancellationToken ct` alır ve iletir.
- Endpoint yalnızca HTTP dönüşümü yapar (`ToHttpResult()`); iş mantığı handler'dadır.
- Handler başka handler çağırmaz; Domain DbContext kullanmaz.
- Yasak: Repository pattern, AutoMapper, lazy loading, magic string, ne yapıldığını anlatan yorum.
- Her yeni handler ve validator için test yazılır.
