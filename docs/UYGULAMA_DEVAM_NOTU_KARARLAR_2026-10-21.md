# OEHS uygulama devam notu — kararlar 1–9
**Tarih:** 21.10.2026  
**Dal:** `feature/official-payment-document-templates`  
**Kapsam:** Kullanıcı onaylı düzeltmeler; ana dala birleştirme/yayınlama yapılmadı.

## Uygulanan kod değişiklikleri

### 1–2. İcmal fiyat farkı çift sayımını giderme
Dosya: `src/OzelEgitimHakedis.Wpf/OehsOfficialDocumentFactory.cs`

Önceki davranışta hizmet sütunu `CalculateWorkbookMonthlyPaymentTotal` ile K (Temel) veya K+T (Özel) tutarını gösteriyordu; bu tutarların içinde I/R fiyat farkı zaten bulunuyordu. Özel Eğitim fark sütununa I+R tekrar yazılabiliyor, Temel Eğitimde ise ikinci blok kesinti tutarını içeriyordu. Böylece hizmet tutarı ile fark sütunu aynı fiyat farkını tekrar toplayabiliyordu.

Yeni davranış:
- İş tutarı sütunu `CalculateWorkbookMonthlyPaymentAmount` ile fiyat farkı hariç sözleşme işi tabanını gösterir.
- Fiyat farkı sütunu `CalculateWorkbookPriceDifference` ile ham I (Temel) veya I+R (Özel) tutarını bir kez gösterir.
- Temel Eğitim'in kaynak icmalindeki ayrı kesinti bloğu korunur; yalnızca tekrar eklenen ham fiyat farkı ayrıştırılır.
- Özel Eğitim için I+R ayrı sütunda yalnızca bir kez gösterilir.
- Satır toplamı bu iki ayrı bileşeni birleştirir; aşağıdaki kesinti mahsupları mevcut yapısını korur.

Bu değişiklik iki programın kendi hücre zincirlerine dayanır; bir programın sonucu diğerine genellenmedi.

### 3. Temel Eğitim W4 → Hakediş Raporu D28
Dosya: `src/OzelEgitimHakedis.Core/Services/ProgressPaymentReportService.cs`

Kabul edilen eşleme:
- `Hesaplama Bilgileri!W4 = O4-V4`
- `Hakediş Raporu!D28` bu W4 net sonucunu temsil eder.

`CalculateReportNetPayable` artık raporun ayrı yuvarlanmış kümülatif alt toplamlarını tekrar çıkararak net üretmiyor; kaynak hesaplamanın `Calculation.NetPayable` değerini döndürüyor. Bu, bir kuruşluk ara yuvarlama farkının D28'e farklı tutar yazmasına engel olur.

### 4. Özel Eğitim AG4 → Hakediş Raporu D28
Aynı servis metodu, programın hesaplama sonucu `Calculation.NetPayable` değerini kullanır:
- `Hesaplama!AG4 = AF4-AE4`
- `Hakediş Raporu!D28` AG4 net sonucuna bağlanır.

Temel ve Özel Eğitim hesap formülleri `ProgressPaymentCalculator` içindeki program türüne özgü davranışlarıyla korunur; sadece rapor net alanının kaynağı eşitlenmiştir.

### 5. Özel Eğitim AP3/AQ3 ay düzeltmesi
Özgün kaynak XLSM'de `İcmal Verileri!AP3` ve `AQ3` aynı formüle bağlıydı:
- AP3: `=Anasayfa!I9` (Aralık)
- AQ3: `=Anasayfa!I9` (Aralık)

Kullanıcı kararı uyarınca düzeltilmiş kaynak kopyasında:
- AP3: `=Anasayfa!I8` (Kasım)
- AQ3: `=Anasayfa!I9` (Aralık)

Düzeltilmiş kopya: `/mnt/data/02-Ozel_Egitim_Hakedis_AP3_Kasim_Duzeltme.xlsm`  
SHA-256: `2afc7ae6b183237e5c6367396a652bac6d67cb4fd552e986a53ed280a6024189`

Özgün dosya değiştirilmeden saklandı:
- SHA-256: `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e`

Uygulama kodu: `src/OzelEgitimHakedis.Core/Services/ProgressPaymentIcmalService.cs` artık Özel Eğitim Aralık için Kasım'ı, Ocak için Aralık'ı önceki ay olarak alır; eski “Aralık eksik / iki Aralık” istisnaları kaldırıldı.

## Test değişiklikleri

- `tests/OzelEgitimHakedis.Tests/CurrentXlsmReportNetDiscrepancyTests.cs`: D28 eşlemesi W4/AG4 hesaplanan net tutarıyla doğrulanacak şekilde güncellendi.
- `tests/OzelEgitimHakedis.Tests/ProgressPaymentIcmalServiceTests.cs`: Aralık önceki kesintisinin Kasım'dan, Ocak önceki kesintisinin Aralık'tan gelmesi beklentileri güncellendi; Eylül–Ağustos 12 aylık dönem geçiş tablosu eklendi.
- `tests/OzelEgitimHakedis.Wpf.Tests/OfficialDocumentOutputTests.cs`: İcmal PDF sayfasında iş tutarı ve fiyat farkının ayrı görünmesi, toplamda çift sayılmaması için beklentiler güncellendi.

## Güvenlik / veri koruma
- Ana dal, yayın veya dağıtım işlemi yapılmadı.
- Özgün XLSM saklandı; AP3 düzeltmesi ayrı kopyada tutuldu.
- Kimlik, banka veya gerçek yüklenici verileri yeni testlere eklenmedi.
- Kullanıcı kimlik bilgileri, sırlar veya makro güvenlik ayarları değiştirilmedi.
- XLSM düzeltmesinde VBA projesinin varlığı kontrol edildi; masaüstü Excel'de VBA derleme/çalışma doğrulaması henüz yapılmış sayılmaz.

## Henüz doğrulanmamış noktalar
- GitHub Actions sonucu, kod değişikliklerini içeren son commit için ayrıca kontrol edilmelidir.
- Windows CI başarılı olsa bile gerçek Microsoft Excel/VBA masaüstü testi bunun yerine geçmez.
- Sekiz nihai PDF'nin gerçek Excel kaynak çıktılarıyla alan bazlı otomatik karşılaştırması ve görsel kabulü tamamlanmadan PDF kabulü 8/8 sayılmaz.
- XLSM hücre değerlerinin gerçek Excel tam yeniden hesaplaması bu notla kanıtlanmış değildir.

## Kapanış ölçütleri
1. Son commit SHA'sına ait CI build/test sonucu alınır.
2. İki programın regresyon testleri ayrı sonuçlandırılır.
3. AP3/AQ3 ve 12 aylık eşleme testi geçer.
4. Sekiz PDF'nin her biri için dönem, İKN, yüklenici, hat, ara/net tutar ve imza alanları Excel çıktısıyla karşılaştırılır; sonra görsel kontrol yapılır.
5. Masaüstü Excel/VBA kabulü ayrıca kaydedilir.
6. PR açık/taslak kalır; ana dala birleştirme/yayınlama için ayrı ve açık onay gerekir.
