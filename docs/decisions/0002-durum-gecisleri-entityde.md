# 0002 — Durum geçiş kuralları entity'de `TryX()` metodu olarak yaşar

Durum: Kabul edildi (2026-09-28, sipariş iptali)

## Bağlam
"Yalnızca Pending sipariş iptal edilebilir" gibi durum kuralları handler'a yazılırsa her handler kuralı yeniden uygular ve kurallar dağılır. Exception ile hata yönetimi ise ekip standardında yasak.

## Karar
- Durum geçişleri entity üzerinde `bool TryX()` metodudur (ör. `Order.TryCancel()`): geçiş geçerliyse durumu değiştirip `true`, değilse hiçbir şeyi değiştirmeden `false` döner.
- Handler `false` sonucunu `AppError.Conflict` (409) olarak döner; mesajda mevcut durumu belirtir.
- `Status` gibi durum alanları private setter'dır; entity dışından değiştirilemez.

## Sonuçlar
- Geçiş kuralları tek yerde toplanır ve DB olmadan domain testiyle doğrulanır.
- Yeni geçişler (ör. `TryComplete()`) aynı kalıpla eklenir.
