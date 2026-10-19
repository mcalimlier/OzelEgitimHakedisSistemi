# PDF 4 × 2 Kabul Matrisi

Tarih: 2026-10-19  
Dal: `feature/official-payment-document-templates`

## Kapsam

Kaynak Excel sayfaları 4/5/6/7, uygulamanın sol menüsüne eklenecek ekranlar değildir. Her program için bu sayfalar, OEHS'nin ürettiği dört PDF'nin kabul referanslarıdır.

| Kaynak program | PDF 4 — Hakediş Raporu | PDF 5 — Hizmet İşleri Hakediş Raporu | PDF 6 — Hizmet İşleri Kabul Tutanağı | PDF 7 — Hakediş İcmali |
|---|---|---|---|---|
| Temel Eğitim | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı |
| Özel Eğitim | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı |

## Her hücre için kabul kontrolü

1. Aynı ihale, sözleşme, yüklenici, hat, hakediş numarası ve dönem kullanılmış olmalı.
2. PDF metin alanları kaynak formun karşılık gelen alanlarıyla karşılaştırılmalı: dönem/tarih, numara, yüklenici, hat/güzergâh, sözleşme bilgileri ve işin süresi.
3. Tutarlar kaynak Excel'in gerçek hesaplanmış hücreleriyle karşılaştırılmalı. Yuvarlama farkları, formül farkları ve yalnızca biçim farkları ayrı sınıflandırılmalı.
4. İcmalde ay sırası, önceki hakediş aktarımı, fiyat farkı ve kesinti davranışı ayrı kontrol edilmeli.
5. İmza/komisyon alanları, metin taşması, sayfa boyutu/yönü ve hücre hizaları görsel olarak kontrol edilmeli.
6. Sonuç her belge için ayrı kaydedilmeli; bir PDF'nin geçmesi diğerlerini geçirmez.

## Şu an eldeki kanıt ve sınırı

GitHub Actions run [39303608475](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39303608475) başarıyla tamamlandı. `OEHS-PDF-Smoke-Review` artefaktı indirildi ve sekiz sentetik PDF incelendi. Sekizinin de sayfa sayısı 1 ve ölçüsü A4 portre.

Sentetik örneklerde görülen program farkları:
- Özel Eğitim kabul tutanağı süreyi `180 İş Günü` olarak gösteriyor; Temel/Ortaöğretim örneği `180` gösteriyor.
- Ekim icmal toplamı Özel Eğitim örneğinde 120,00 TL; Temel/Ortaöğretim örneğinde 125,00 TL.

Bunlar sentetik smoke verisinin gözlemleridir; tek başına kaynak Excel'e uygunluğu kanıtlamaz ve otomatik olarak hata sayılmaz. İncelemede ayrıca:
- Hakediş Raporu'ndaki KDV açıklaması Özel Eğitim'de taşıma %10 + rehber %20, Temel/Ortaöğretim'de yalnızca taşıma %10 olarak üretiliyor.
- Sentetik örnekte damga vergisi tutarı Özel Eğitim'de 1,00 TL, Temel/Ortaöğretim'de 1,04 TL.
- Bu farklar programların ayrı hesaplama zincirleriyle ilişkili olabilir; gerçek aynı-girdi kaynak Excel sonucu görülmeden değişiklik yapılmayacak.

## Mevcut otomatik test kapsamı (Excel kabulü yerine geçmez)

- `tests/OzelEgitimHakedis.Wpf.Tests/OfficialDocumentOutputTests.cs`: dört ayrı belge üretimi, A4 portre/tek sayfa, temel alanlar, imza metinleri, icmalde Eylül–Ekim takvim sırası ve sentetik tutar kontrolleri.
- `tests/OzelEgitimHakedis.Tests/ProgressPaymentIcmalServiceTests.cs`: Eylül–Temmuz dönemi, Kasım için önceki ay aktarımı, eksik başlık/lookup sınırı, program ve hat izolasyonu.
- `tests/OzelEgitimHakedis.Tests/CurrentXlsmSourceParityTests.cs` ve `CurrentBasicWorkbookDiscrepancyTests.cs`: kodun kaydedilmiş kaynak hücre sonuçlarına karşı regresyon vektörleri. Test açıklamaları bunların masaüstü Excel/VBA yeniden hesaplaması olmadığını açıkça belirtiyor.

Son dokümantasyon commit'i `c21315852b0a02276a27a47d8ce6d7b49374e939` için 19.10.2026 itibarıyla hem [push CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39306949094) hem de [PR CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39306955405) çalışıyor; sonuç henüz tamamlanmış değil.

## Kapanış koşulu

Matristeki sekiz hücrenin her biri için gerçek Excel masaüstü/VBA ile yeniden hesaplanmış referans sonuçlar ve aynı girdilerle OEHS PDF'si mevcut olmalı. Her eşleşme alan bazında kayıt altına alınmalı. Excel formülü ile uygulama arasında fark bulunursa, kaynak hücre/formül, gerçek sonuç ve test kanıtı belgelenmeden hesaplama davranışı değiştirilmemeli.

CI başarısı, sentetik PDF smoke sonucu veya yalnızca PDF'nin açılması bu kabul kapısını kapatmaz.
