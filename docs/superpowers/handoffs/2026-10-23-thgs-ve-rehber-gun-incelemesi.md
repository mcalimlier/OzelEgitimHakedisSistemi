# Hatlar → Hakediş Oluştur: Döneme Esas THGS ve Rehber Gün

Tarih: 23.10.2026

## Talep ve başlangıç

- Hatlar panelindeki Taşıma İş Günü: **Döneme Esas Taşıma Hakediş Gün Sayısı (Döneme Esas THGS)** olarak adlandırılacak.
- Seçilen hattın bu değerinin hakedişin Excel-hibrit tablosuna aktarımı doğrulanacak.
- Rehber Yıllık İş Günü ile tablodaki Rehber Gün arasındaki gerçek kod ilişkisi açıklanacak.
- Etkin depo: `mcalimlier/OzelEgitimHakedisSistemi`.
- Dal: `feature/official-payment-document-templates`; PR #3 açık/taslak.
- Başlangıç HEAD: `943910e965ec4cc34131abf58cd08f635f5980e7`.
- Başlangıç Windows CI: `39849366008`, success. Eski devam notundaki commit yerine uzaktaki güncel dal doğrulandı.

## 1. Taşıma günü kök neden ve veri zinciri

| Aşama | Gerçek kaynak/hedef |
| --- | --- |
| Hat girişi | `MainWindow.xaml`: `RouteTransportWorkDaysTextBox` |
| Hat kaydı | `AddRouteButton_Click` → `RouteDraft.TransportWorkDays` |
| Diske yazma/okuma | `SavePersistedData` / `LoadPersistedData` → `RouteData.TransportWorkDays` |
| Hat seçimi | `ProgressRouteComboBox_SelectionChanged` → `UpdateHybridRouteDraftRows` |
| Hatalı nokta | Yeni satırda `WorkDays = 0`; hattın kayıtlı taşıma günü kullanılmıyor |
| Düzeltilen nokta | Yeni satırda `WorkDays = route.TransportWorkDays` |
| Excel-hibrit hücresi | `MultiRoutePaymentGrid`, Taşıma Gün sütunu → `MultiRoutePaymentDraftRow.WorkDays` |
| Canlı önizleme | `CreateInput` → `ProgressPaymentCalculator.Calculate`; taşıma hizmet tutarı = gün × günlük fiyat |
| Hakediş kaydı | `SaveMultiRouteProgressPayment` → `ProgressPaymentInput.WorkDays` → geçmiş kaydı/diske yazma |

Aktarım yeni satırın ilk kuruluşunda yapılır. Hücrede yapılan dönemsel düzenlemeler aynı taslak içinde yeniden seçimde korunur. Tabloyu yeniden kurmak mevcut taslak günlerini hat varsayılanıyla ezmez. Günlük fiyat ve diğer ödeme formülleri değişmez.

Etiket hat girişinde, ilgili doğrulama mesajında ve hat özetinde güncellendi. Gün × fiyat kartındaki yanıltıcı “Taşıma Yıllık” ifadesi dönem anlamına uyarlandı; veri alanının adı ve mevcut dosya şeması korunur.

## 2. Rehber alanlarının ilişkisi

**Aynı canlı alan değiller; ayrı kayıtlarda aynı özellik adı kullanılıyor. Normal yeni hakedişte otomatik bağlı değiller, düzeltme yollarında bağlılar.**

| İşlem | Kod davranışı |
| --- | --- |
| Hatlar / Rehber Yıllık İş Günü | `RouteGuideWorkDaysTextBox` → `RouteDraft.GuideWorkDays` → `RouteData.GuideWorkDays` |
| Yeni hakedişte hat seçme | `UpdateHybridRouteDraftRows`: `GuideWorkDays = 0`; yıllık gün aktarılmaz |
| Tabloda Rehber Gün düzenleme | `MultiRoutePaymentDraftRow.GuideWorkDays` → `ProgressPaymentInput.GuideWorkDays` |
| Rehber hizmet tutarı | `ProgressPaymentCalculator.Calculate`: `input.GuideWorkDays * input.GuideDailyPrice` |
| Mevcut hakedişte hat düzeltme | `ProgressPaymentPropagationService.RebuildForRoute`: `GuideWorkDays = route.GuideWorkDays` (Özel Eğitim) |
| Arşivde hat düzeltme | `SaveArchivedRouteCorrection`: formdaki rehber günü hem arşiv hat kaydına hem hakediş girdisine yazılır |
| Canlı düzeltme önizlemeleri | `TryCreateArchiveCorrectionPreview` / `TryCreateHistoryCorrectionPreview` içinde hat formundaki rehber günü ödeme girdisine atanır |
| Temel/Ortaöğretim | Rehber hizmeti kullanılmaz; hesap girdisi rehber günü/fiyatını 0 tutar |

### Açık risk

“Yıllık” etiketiyle girilen 180 gün ile hakedişte fiilî 18 günlük rehber hizmeti ayrı kavramlardır. Yeni hakedişte 180 gün tabloya otomatik aktarılmaz. Fakat yukarıdaki hat/arşiv düzeltme yollarında yıllık değer dönem girdisine yazılabilir; aynı özellik adının kullanılması bu kavram karışıklığını gizliyor. Genel hat düzeltmesi eşleşen birden fazla geçmiş hakedişi de yeniden hesaplayabilir.

Bu inceleme, yıllık değerin gerçekten puantajla doğrulanmış dönem gününe nasıl çevrileceğine dair iş kuralı üretmez; yıllık değeri aya bölmez, rehber alanının anlamını sessizce değiştirmez. Kullanıcının ikinci maddesi ilişkiyi bulma talebi olduğu için rehber düzeltme davranışı bu değişiklikte değiştirilmedi. Sonraki ayrı düzeltme, yıllık hat bilgisini dönem girdisinden ayırmalı ve sırf hat bilgisi değiştiği için kayıtlı dönem günlerini değiştirmemelidir; hedefli hat/arşiv düzeltme iş akışı ayrıca ele alınmalıdır.

## 3. Kabul sınırları

- Ay değişiminde mevcut `ResetMonthlyInputsForNewMonth` taşıma/rehber gününü ve parasal taslakları sıfırlar. Önceki ayın puantajı yeni aya taşınmaz. Bu koruma değişmedi; ilk hat seçimindeki varsayılan aktarımı ile ay değiştirme aynı işlem değildir.
- Mevcut dosyalarda geçmişte yıllık anlamda girilmiş taşıma günleri otomatik dönüştürülmedi. Seçili sözleşme ayının takvim kapasitesini aşan günler mevcut kayıt doğrulamasıyla reddedilir. Bu kontrol gerçek çalışma günü/puantaj doğrulaması yerine geçmez.
- Geçmiş ödeme ve arşivler bu yeni varsayılan nedeniyle topluca değiştirilmez. Mevcut hat düzeltme yayılım davranışı yukarıda ayrıca belgelenmiştir.
- Kaynak XLSM/VBA dosyaları, iki programın formülleri ve resmî PDF şablonları değiştirilmez.
- Yerel ortamda .NET SDK/Windows WPF çalışma zamanı yok; gerçek testler Windows CI üzerinde çalıştırılır. Fiziksel Windows masaüstü ve Excel/VBA kabulü ayrıca açıktır.

## 4. Test ve devam durumu

Durum: Windows CI testleri bekleniyor. Sonuçlar tamamlandığında bu bölüm güncellenecek.

Regresyon kapsamı:

- İki programda ayrı hatlara ait 18,5 ve 12 günün doğru satırlara gelişi.
- Aynı taslakta 17 güne düzenlenen satırın seçimden çıkartılıp geri alındığında korunması.
- Rehber yıllık 180 günün yeni hakedişte 0 olan dönem gününe otomatik taşınmaması.
- Tek ve çok hat için 16,5 / 17,5 günün hesap girdisi ve diskteki ödeme kaydına kadar korunması.
- Mevcut hat bazlı düzenleme, geçersiz hücre, ay geçişi, takvim kapasitesi, hesaplama ve PDF regresyonları.

Sıradaki iş: rehber yıllık/dönem ayrımının hat/geçmiş/arşiv düzeltme yollarında iş kuralına uygun olarak giderilmesi. Ana dala birleştirme veya üretim yayını yapılmaz.
