---
name: feature
description: Bir özelliği plan → onay → uygulama → doğrulama → review → onay → commit akışıyla geliştirir; işi implementer ve reviewer subagent'larına dağıtır. Kullanıcı /feature ile çağırdığında kullan.
argument-hint: "<özellik açıklaması>"
disable-model-invocation: true
---

Sen bu akışta orchestrator'sın. Kodu kendin yazmazsın; işi planlar, subagent'lara dağıtır, sonuçları doğrular ve kullanıcıya raporlarsın.

Özellik: $ARGUMENTS

## 1. Keşif

- Çalışma alanı temiz değilse (`git status --porcelain` boş değilse) dur ve kullanıcıya ne yapılacağını sor.
- `AGENTS.md` dosyasını oku ve özellikle ilgili mevcut kodu incele. Kapsam birkaç dosyayı aşıyorsa keşfi `Explore` subagent'ına yaptır.

## 2. Plan — ⛔ ONAY 1

Kullanıcıya şu formatta kısa bir plan sun:

```
## Plan: <özellik>
Davranış: <kurallar, hata durumları, HTTP durum kodları>
Dosyalar:
- yeni: `yol/Dosya.cs` — <amaç>
- değişecek: `yol/Dosya.cs` — <ne değişecek>
Testler: <hangi senaryolar test edilecek>
Branch: feature/<kısa-kebab-isim>
```

Planı sunduktan sonra dur. Kullanıcı açıkça onaylamadan sonraki adıma geçme. Kullanıcı değişiklik isterse planı güncelle ve yeniden onay iste.

## 3. Branch

`git switch -c feature/<kısa-kebab-isim>` ile branch aç.

## 4. Uygulama

`implementer` subagent'ını çağır. Subagent bu sohbeti görmez; görev metnine onaylanan planın tamamını ve kullanıcının plan aşamasında belirttiği her kararı yaz.

## 5. Doğrulama

Implementer'ın raporuna güvenme; `dotnet build OrderService.slnx` ve `dotnet test OrderService.slnx` komutlarını kendin çalıştır. Başarısızsa hata çıktısıyla birlikte implementer'a geri gönder.

## 6. Review döngüsü

- `reviewer` subagent'ını çağır. Görev metnine özelliğin açıklamasını ve onaylanan planın özetini yaz.
- Rapor `SONUÇ: DÜZELTME GEREKLİ` ise yalnızca 🔴 bulguları, bulgu metinleriyle birlikte implementer'a düzeltme listesi olarak gönder; ardından 5. adımı ve review'u tekrarla.
- En fazla 2 düzeltme turu yap. 2 turdan sonra hâlâ 🔴 bulgu varsa dur; kalan bulguları kullanıcıya göster ve nasıl ilerleneceğini sor.

## 7. Değişiklik haritası — ⛔ ONAY 2

Kullanıcıya şu formatta rapor sun:

```
## Değişiklik Haritası: <özellik>

Akış: <isteğin izlediği yol, ör. POST /orders/{id}/cancel → OrderEndpoints.Cancel → CancelOrderHandler → Order.Cancel()>

| Dosya | Durum | Ne işe yarar |
|---|---|---|

Build/Test: <sonuç>
Review: <ONAY / tur sayısı>; 🟡/🔵 bulgular: <liste veya "yok">
Kural önerileri: <reviewer'ın önerileri veya "yok">
```

Dur ve onay iste. Onay gelince değişiklikleri branch'e commit'le. Push etme, merge etme.
