# OEHS — Ön Denetim Risklerinin Uygulama ve Doğrulama Kaydı

**Tarih:** 15.10.2026  
**Kapsam:** İlk beş riskin teknik durumu/düzeltmeleri ve hesaplama sabitlerinin onaylı yönetimi.

> Bu kayıt teknik uygulama ve test bulgularını özetler; tek başına %100 mevzuat uyumu veya belirli bir ödeme dosyasına ilişkin kesin hukuki görüş değildir. İlgili sözleşme, idari/teknik şartname ve ödeme tarihinde yürürlükteki mevzuat ayrıca doğrulanmalıdır.

## R-01 — Fiyat farkının iki kez hesaba katılması

**Sonuç: Önleyici hesaplama mantığı ve regresyon testleri mevcut.**

`ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentAmount`, A satırını hizmet tutarları ve iskontolar üzerinden hesaplayıp ham I+R fiyat farkını ayrı B satırında tutar. Böylece fiyat farkının A içine yazılıp B satırında yeniden eklenmesi önlenir. C=A+B ve E=C−D testlerle kontrol edilir.

İlgili testler:
- `ProgressPaymentWorkbookFormulaTests.ReportTotals_ShouldKeepCumulativeWorkAndPriceDifferenceSeparate`
- `ProgressPaymentReportTests` içindeki rapor toplamı ve negatif fiyat farkı senaryoları.

Kabul öncesinde gerçek XLSM'deki D6, D7, D8, D10 ve D11 hücreleri aynı veri örneğiyle izlenmelidir. Kaynak Excel'in hücre anlamları ile C# eşlemesi arasındaki fark, sözleşmeye uygunluk teyit edilmeden kapatılmamalıdır.

## R-02 — Negatif fiyat farkı ve ek kesin teminat

**Sonuç: Kaynak XLSM formülü ve mevcut C# kodu işaretli sonucu koruyor; mevzuatla yorum farkı açık karar olarak tutuluyor.**

Güncel kaynak formülleri:
- Temel Eğitim: `Hesaplama Bilgileri!T4 = I4*6/100`.
- Özel Eğitim: `Hesaplama!AC4 = (I4*6/100) + (R4*6/100)`.
- Her iki formülde de ilgili fiyat farkı terimleri ayrı ayrı %6 ile çarpılır; J/S iskonto tutarları teminat matrahından düşülmez.

Bu formüllerin doğrudan uygulanması, negatif I veya R girdisi olduğunda negatif ek kesin teminat bileşeni üretebilir. Mevcut `ProgressPaymentCalculator` bunu `MAK(0; I+R)` biçiminde sınırlandırmaz. Kaynak formüle uygunluk testleri de bu işaretli davranışı doğrular:
- `ProgressPaymentCalculatorTests.NegativePriceDifference_ShouldMatchExcelSignedGuaranteeFormula`
- `ProgressPaymentCalculatorTests.MixedSignedPriceDifferences_ShouldCalculateEachExcelPriceDifferenceAtSixPercent`
- `CurrentBasicWorkbookDiscrepancyTests.CurrentBasicRows5And6_ShouldMatchCalculationFormulaChain`
- `CurrentXlsmSourceParityTests` içindeki Özel Eğitim satır 4–6 testleri.

**Açık karar / risk:** 4735 sayılı Kanun'un 12'nci maddesi ek kesin teminatı fiyat farkı nedeniyle sözleşme bedelinde artış meydana gelmesi koşuluna bağlar. Bu mevzuat ifadesi ile kaynak Excel'in negatif fiyat farkına da %6 uygulayan cebirsel formülü aynı davranış değildir. Resmî hesaplama davranışı, yetkili karar ve işlem tarihindeki sözleşme/mevzuat incelemesi olmadan sessizce `MAX(0, I+R)` formülüne çevrilmemelidir. Mevcut uygulama kaynak Excel formülünü izler; negatif fiyat farkı senaryosu ayrı kabul/risk maddesi olarak kalır.

Kaynak:
https://ekap.kik.gov.tr/EKAP/Vatandas/KurulKararGoster.aspx?KararId=5a55fc2e4d77a80d4646eb11537c79a3f55aca0b0f11934f4313f5be1b4ea587

## R-03 — Hizmet İşleri Hakediş Raporu örnek numarası

**Sonuç: Örnek numarası düzeltildi ve PDF regresyon testi eklendi.**

Mahalli İdareler Harcama Belgeleri Yönetmeliği'nin 6'ncı maddesi yapım işleri hakediş raporunu Örnek 4, hizmet işleri hakediş raporunu Örnek 5 olarak ayırır. OEHS içindeki ayrı “Hizmet İşleri Hakediş Raporu” imza altındaki örnek numarası Örnek 5 olarak güncellendi. Diğer “Hakediş Raporu” sayfasındaki `M.Y.H.B.Y. Örnek No :4/10` ayrı rapora ait olduğundan değiştirilmedi.

Kaynak: https://www.lexpera.com.tr/resmi-gazete/metin/mahalli-idareler-harcama-belgeleri-yonetmeligi-26614

**Sınırlama:** Örnek numarasının düzeltilmesi, şablonun Yönetmelik'teki tüm sayfa ve ekleriyle bire bir uyumlu olduğunu tek başına kanıtlamaz. Ödeme biriminin kullanacağı resmî form setiyle karşılaştırma yapılmalıdır.

## R-04 — Aylık veri silme işleminin tüm dönemleri etkilemesi

**Sonuç: Kaynak Excel'deki tehlikeli silme makrosu OEHS'ye taşınmadı.**

Kaynak VBA'da tarif edilen `AyVerileriniSil` makrosunun geniş dönem aralıklarını topluca temizleme davranışını çağıran C# işlemi yoktur. OEHS ana veri dosyası önceki sürümü `.bak` olarak saklar; arşiv düzeltmeleri revision kayıtlarıyla yürütülür. Kaynak makronun toplu silme davranışını uygulamaya taşımamak önleyici kontroldür.

Kabul testinde tek dönem silme/iptal akışında diğer dönemlerin korunması, ekranların gerçek kullanıcı kabul testiyle doğrulanmalıdır.

## R-05 — Takvim günü ve iş günü ifadelerinin karışması

**Sonuç: Belge etiketi ve belirsiz süre davranışı kodda düzeltilmiş; testle korunuyor.**

Kabul tutanağı `Sözleşmeye göre işin süresi (iş günü)` etiketini kullanır. Süre sözleşmedeki `ContractWorkDays` alanından alınır; takvim tarihlerini çıkarıp iş günüymüş gibi göstermez. Yetkili iş günü değeri bulunmuyorsa belge `-` gösterir.

`OfficialDocumentOutputTests` kabul tutanağı etiketini ve “takvim günü” ifadesinin bulunmadığını doğrular.

## R-06 — KDV, tevkifat, damga vergisi ve ek kesin teminat oranlarını merkezi yönetme

**Sonuç: Ayrı Hesaplama Sabitleri paneli ve denetim geçmişi eklendi.**

- AYARLAR ekranından ayrı **Hesaplama Sabitleri** paneli açılır.
- Taşıma/rehber KDV oranları, taşıma/rehber tevkifat payları, damga vergisi ve ek kesin teminat oranı yüzde olarak değiştirilebilir.
- Bütün oranlar %0–%100 aralığında doğrulanır.
- Kaydetmeden önce açık kullanıcı onayı alınır.
- İşlemi yapan kişi ve gerekçe zorunludur.
- Yeni değerler uygulama veri dosyasına yedekli şekilde kaydedilir.
- Değişiklik zamanı, eski/yeni değer, kullanıcı ve gerekçe geçmişte saklanır.
- Her hesaplama sonucunda kullanılan oran seti de saklanır.
- WPF hesaplama ve yeniden hesaplama çağrıları güncel kaydedilmiş sabitleri kullanır; PDF üzerindeki KDV/tevkifat açıklamaları ilgili hesabın oranlarına göre üretilir.
- Geçmiş ödeme/arşiv tutarları denetim izi korunarak sessizce değiştirilmez; eski kayıt ancak açık bir düzeltme/yeniden hesaplama işleminde güncellenir.

İlk değerler kaynak Excel'deki mevcut oranlardır; mevzuata uygunluk teyidi değildir. Oran değişikliği öncesinde geçerlilik tarihi ve sözleşme/hizmet türüne uygulanabilirlik mali mevzuat uzmanı tarafından doğrulanmalıdır.

## Test ve kabul durumu

Eklenen testler:
- Değiştirilebilir oranlarla KDV, tevkifat, damga vergisi ve ek kesin teminat hesapları.
- Geçersiz sabitlerin hesaplamaya alınmaması.
- Hesap sonucunda kullanılan oran setinin saklanması.
- Sabitler ve denetim geçmişinin veri dosyasına yazılıp geri okunması.
- Hizmet İşleri Hakediş Raporu için Örnek 5 ibaresi ve dinamik oran açıklamaları.

GitHub Actions ve Windows kabul testleri ayrıca çalıştırılıp doğrulanmalıdır. CI başarılı olsa bile uygulama açılışı, sabit değiştirme/onay, JSON+yedek, yeni hesap ve dört tek sayfalı A4 PDF gerçek bir örnek hakedişle sınanmalıdır.
