# Hakediş Oluştur yayın öncesi doğrulaması — 25.10.2026

## Sonuç

Hakediş Oluştur paneli, güncel iki kaynak Excel dosyasından alınan girdilerle kontrol edildi. 24 senaryoda 236 hesap alanı, WPF satır önizlemeleri ve sekiz hattın gerçek hesapla/kaydet akışı doğrulandı. Ayrı masaüstü deneme uygulamasında da düğme kullanılarak 3 Özel Eğitim ve 5 Temel Eğitim hakedişi oluşturuldu; diskteki 76 finansal alan ve 20 toplu alan Excel ile eşleşti. Toplu alanlarda Excel'in kendi `SUM` ve `ROUND` sonuçları kullanıldı. Test edilen örneklerde beklenmeyen hesaplama farkı bulunmadı.

İKN, firma ve dönem seçimlerinin hizası düzeltildi. Başlık, etiket ve giriş satırları ortak WPF ölçü gruplarına bağlandı; üç seçim kutusu 34 DIP yüksekliğinde. Önceki yerleşimde dar ekranda yaklaşık 8 DIP düşey fark vardı. 1024×600, 1366×768 ve 1920×1080 boyutlarında üst kenar/yükseklik farkları en çok 1 DIP kabulüyle test edildi. Alt hesap formundaki altı metin kutusu çifti de aynı boyutlarda ölçüldü; bunlarda ek üretim değişikliği gerekmiyordu.

Bu sonuç, aşağıda açıkça listelenen Excel/VBA ve nihai PDF kabul işlerini tamamlanmış saymaz. Uygulama yayımlanmadı; gerçek kayıtlar ve kaynak Excel dosyaları değiştirilmedi.

## Kaynaklar ve yöntem

| Program | Kaynak dosya | SHA-256 |
|---|---|---|
| Temel / Ortaöğretim | 03-Temel Eğitim Taşıma Ödeme SON.xlsm | `37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296` |
| Özel Eğitim | 02-Özel Eğitim Hakediş Ödeme SON.xlsm | `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e` |

Microsoft Excel 16.0 kullanıldı. Kaynaklar salt okunur açıldı; olaylar ve makrolar kapalı tutuldu. `CalculateFullRebuild` ile formüller yeniden hesaplandı. Ek senaryolar yalnız açık çalışma kitabının bellekteki giriş hücrelerinde denendi; dosyalar kaydedilmeden kapatıldı. Kaynak hash'leri işlem sonunda tekrar kontrol edildi.

Temel Eğitim: `Hesaplama Bilgileri` satır 4–8; Özel Eğitim: `Hesaplama` satır 4–6. Her programa ayrıca KDV istisnası, kesirli gün, negatif fiyat farkı, iskonto/borç/ceza/destek, sıfır girdi, hassas fiyat, 30 günlük dönem ve yalnız fiyat farkı senaryoları uygulandı. Toplam 24 senaryo oluştu.

Ham hesap karşılaştırmasının toleransı 0,000001 TL; iki ondalıklı para gösterimi ayrıca Excel'in kendi `ROUND` sonucuyla karşılaştırıldı. Excel'in ikili `Value2` sonucunu JSON'a aktarıp .NET'te tekrar yuvarlamak, yarım kuruş sınırında yanlış karşılaştırma üretebildiğinden bu yöntem kullanılmadı. Kaydedilmiş decimal değerler ve gerçek WPF sayı biçimlendirmesi de doğrulandı.

Denetim aracında kaynak girdiler Excel'in 15 anlamlı basamağıyla (`G15`) okunur. Örneğin hücredeki 7365,35 girdisini `Value2` üzerinden 7365,349999999999 gibi yeni bir decimal girdiye çevirmek, beş hattın toplam teminatında yanlış bir yarım-kuruş karşılaştırması oluşturuyordu. Bu aktarım kusuru denetim aracında düzeltildi. Günlük fiyatın 19539,1608 gibi gerçek basamakları korunur; uygulama formüllerine veya tüm girdilere iki ondalık yuvarlama eklenmedi. Çıktı hücreleri ise bağımsız ham değer denetimi için tam `Value2` ile saklanır.

## Gerçek masaüstü uygulamasında kayıt sonuçları

Kasım 2026 dönemi, hayalî firma ve hat adlarıyla ayrı `OEHS_DENEME_PAKETI.flag` veri kökünde çalıştırıldı. Kaynaklardan yalnız deneme için gereken sayısal veriler alındı. Kullanıcının normal uygulama verileri kullanılmadı.

| Program | Excel satırı | Excel neti | Programda kaydedilen net | Sonuç |
|---|---:|---:|---:|---|
| Temel / Ortaöğretim | 4 | 332.516,07 TL | 332.516,07 TL | Eşleşti |
| Temel / Ortaöğretim | 5 | 116.186,11 TL | 116.186,11 TL | Eşleşti |
| Temel / Ortaöğretim | 6 | 109.361,28 TL | 109.361,28 TL | Eşleşti |
| Temel / Ortaöğretim | 7 | 88.088,37 TL | 88.088,37 TL | Eşleşti |
| Temel / Ortaöğretim | 8 | 54.007,57 TL | 54.007,57 TL | Eşleşti |
| Özel Eğitim | 4 | 61.508,94 TL | 61.508,94 TL | Eşleşti |
| Özel Eğitim | 5 | 50.492,87 TL | 50.492,87 TL | Eşleşti |
| Özel Eğitim | 6 | 49.385,12 TL | 49.385,12 TL | Eşleşti |

Birleşik netler: Temel / Ortaöğretim **700.159,40 TL**, Özel Eğitim **161.386,94 TL**. Toplamlar, iki ondalıklı satır görüntülerini toplayarak değil, Excel'deki gibi ham tutarların toplamını para biçiminde göstererek hesaplanır. Özel Eğitim'de yuvarlanmış üç satırın elle toplamı 161.386,93 TL iken ham toplamın doğru gösterimi 161.386,94 TL'dir; ara yuvarlama eklenmedi.

## Rapor ve icmal kapsamı

Mevcut `Compare-UploadedXlsmWithOehs.ps1` masaüstü Excel ile ayrıca çalıştırıldı: 38 alan eşleşti, dört alan önceki kullanıcı kararlarıyla belgelenmiş fark olarak kaldı; beklenmeyen fark yoktu. Bu dört alan, kaynak icmaldeki fiyat farkının tekrar sayılmasına ilişkin Temel/Özel Eğitim C8 ve E8 alanlarıdır. Bunlar eşleşen alan sayısına katılmadı.

Hesaplama sayfası neti ile resmî rapor neti ayrı alanlardır. Örneğin Temel Eğitim ilk satırın W4 gösterimi 332.516,07 TL, kaynak rapor D28 gösterimi 332.516,08 TL olabilir. Mevcut program-özel rapor yuvarlama zinciri korundu; kontrol sırasında finansal formüller değiştirilmedi.

## Testler ve yeniden üretme

- Standart Release tam çözüm: **439/439** (Core 265, WPF 174).
- Son para biçimlendirme/Excel/yerleşim testleri: **3/3**; aynı 24 senaryo ve sekiz kaydın tamamını döngüyle kontrol eder.
- Dört CI statik kontrolü başarılı.
- PDF smoke: **24/24 dosya** üretildi ve smoke betiği tamamlandı. Otomatik üretim, kaynak Excel/VBA çıktısıyla 8/8 nihai görsel kabul anlamına gelmez.
- Derleyici analiz uyarıları vardır; başarısız test yoktur. Öz inceleme yapıldı; bağımsız ajan incelemesi bu oturumda mevcut değildi.

Yeniden üretilebilir sayısal kaynak vektörleri: `tests/OzelEgitimHakedis.Wpf.Tests/Fixtures/excel-recalculated-2026-10-25.json`. Bunlarda gerçek firma/kişi/banka verisi bulunmaz. WPF denetimleri: `PaymentReleaseAuditTests.cs`.

Excel masaüstü denetimi için önce Release derleyin, ardından:

```powershell
./scripts/Verify-PaymentPanelExcelSources.ps1 -OutputRoot .superpowers/payment-audit
```

Dosyalar başka yerdeyse `-BasicWorkbookPath` ve `-SpecialWorkbookPath` verin. Çıktılar ham/gösterilen sonuçları ve kaynak Excel `SUM` toplamlarını içerir. VBA çalıştırılmaz.

Bu oturumun ayrıntılı JSON/CSV, tam test kayıtları, hizalanmış panel render'ı ve özel deneme uygulaması `.superpowers/release-2026-10-25/` altındadır. Normal uygulama verilerine deneme kaydı eklenmedi; yalnız ayrı QA penceresi kapatıldı.

Kalıcı sayısal kanıt: [Gerçek uygulama karşılaştırması JSON](evidence/2026-10-25-native-payment-comparison.json), [satır karşılaştırması CSV](evidence/2026-10-25-native-payment-comparison.csv). Excel'in 20 toplu sonucu da test fixture'ında saklanır: `excel-source-totals-2026-10-25.json`.

> Güncel ek kabul: [VBA / PDF / gerçek DPI raporu](YAYIN_KABUL_VBA_PDF_DPI_2026-10-25.md). Aşağıdaki %68 tablosu 3409b41 aşamasının tarihsel durumudur.

## Kaç çekirdek var? Önceki yayın kabul ilerlemesi

Tek fiziksel `OzelEgitimHakedis.Core` kütüphanesi vardır. Projenin önceki kabul panosunda tanımlanan **beş işlevsel çekirdek** aşağıda izlenir. Her çekirdekte beş somut kabul kapısı vardır; yüzde = kapanan kapı / 5. Eşit ağırlıklı bu yayın kabul ölçümü, kod miktarı veya otomatik test geçme oranı değildir.

| İşlevsel çekirdek | Kapanan kapı | Yayın kabulü | Açık kalan kapılar |
|---|---:|---:|---|
| 1. Temel Eğitim hesaplama | 4/5 | **%80** | Kaynak VBA aktarımı çalıştıktan sonra yeniden hücre kabulü |
| 2. Özel Eğitim hesaplama | 4/5 | **%80** | Kaynak VBA aktarımı çalıştıktan sonra yeniden hücre kabulü |
| 3. Hakediş rapor zinciri | 3/5 | **%60** | Tüm resmî alanların kaynak çıktıyla karşılaştırılması; çok dönemli VBA aktarımından sonra rapor kabulü |
| 4. İcmal ve aktarım | 3/5 | **%60** | Makro sonrası ay aktarımı/E21 doğrulaması; düzeltilmiş kaynakla tam alan kabulü |
| 5. Dört PDF ve WPF akışı | 3/5 | **%60** | Aynı girdiden iki programın sekiz Excel/OEHS PDF'sinin görsel kabulü; gerçek %100/%125/%150 Windows DPI kabulü |

**Genel yayın kabulü: 17/25 kapı = %68.** Bu sayı yayın onayı değildir.

Kapanan kapıların kanıtı:

1–2: kaynak dosya kimliği; güncel kaynak regresyonları; masaüstü Excel formül/sınır durumları; gerçek WPF hesapla/kaydet ve disk karşılaştırması.

3: kaynak formül zincirinin belgelenmesi; rapor regresyonları; her programın D6/D7/D8/D10/D11/D15/D26/D28 masaüstü Excel karşılaştırması.

4: kaynak farklarına ilişkin kayıtlı karar; ay/kimlik/fiyat farkı regresyonları; kaynak icmal farklarının masaüstü Excel'de açık sınıflandırılması.

5: dört belge için otomatik finansal/yerleşim testleri; WPF kayıt/doğrulama ve responsive yerleşim kontrolleri; geçmiş/arşiv/yedekleme/dışa aktarma akışlarının mevcut regresyonları.

Yayın öncesi kalan iş: açık VBA ve nihai belge kabul kapılarını kapatıp aynı son sürüm için sonuçları kaydetmek. Bu denetimle tüm uygulama için yüzde yüz hatasızlık veya tamamlanmış yayın kabulü ilan edilmedi.
