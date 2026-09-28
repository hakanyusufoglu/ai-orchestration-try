# 0001 — Değer döndürmeyen command'lar `Result<Unit>` döner

Durum: Kabul edildi (2026-09-28, sipariş iptali)

## Bağlam
Update/Delete/Cancel gibi command'lar başarıda değer döndürmez. `Shared` içinde değersiz bir `Result` tipi yok; `ToHttpResult()` başarılı `Result<T>` için değer null değilse 200 döner.

## Karar
- Değer döndürmeyen command'lar `IRequest<Result<Unit>>` (MediatR `Unit`) kullanır; handler başarıda `Result.Ok(Unit.Value)` döner.
- Endpoint başarıda `Results.NoContent()` (204), hatada `result.ToHttpResult()` döner.

## Sonuçlar
- `Shared`'e yeni tip eklenmez; kalıp her feature'da aynıdır.
- Endpoint'te başarı dalı elle yazılır. Bu tekrar can sıkarsa `Shared`'e değersiz `Result` + `ToHttpResult()` overload'u eklemek yeni bir ADR ile değerlendirilir.
