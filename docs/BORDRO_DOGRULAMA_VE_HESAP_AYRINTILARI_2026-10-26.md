# Bordro karşılaştırması ve kayıtlı hakediş hesap ayrıntıları — 26.10.2026

Başlangıç kaydı: `d9c1472`, dal: `feature/official-payment-document-templates`.

Devam kontrolü: `GERCEK_PENCERE_BORDRO_KABULU_2026-10-26.md`. Bu ilk rapordaki satır içi buton yerleşimi sonradan kullanıcı isteğiyle alt eylem çubuğuna taşındı; gerçek pencere üzerinden 42 kayıt ve 168 PDF denetimi eklendi.

## Kaynak ve yöntem

- `5-Taşıma Bordro GENEL (1).xlsx`: SHA256 `1214d4a22611bd3c7b8332f2bc6fd1084497d8dd62165e9d3b264e87960ed669`
- `5-Taşıma Bordro.xlsx`: SHA256 `cfe8ce2ddc2098072578560bebefa2c4b6443fc6a5edff3c8f23f78f1959ae27`
- Her dosyada 18 taşıma ve 3 özel eğitim veri satırı: iki dosya boyunca 42 satır, 426 karşılaştırma alanı. Tekrarlanan örnekler ayrı kaynak adresleriyle denetlendi; bunlar 42 farklı gerçek hakediş olduğu iddiası değildir.
- Toplam ve alt toplam satırları yeni hakediş girdisi olarak kullanılmadı. Aynı hattın farklı gün/fiyat parçaları ayrı tutuldu.
- Çalışma günleri, günlük fiyatlar, iskontolar, fiyat farkları, rehber alanları, borçlar ve kesintiler doğrudan kaynak hücrelerden alındı. Boş sayısal girişler 0 kabul edildi.
- Kaynakta boş bırakılan KDV/tevkifat hücreleri hata sayılmadı. Kullanıcı bu boşlukların tevkifat uygulanmayan durumlarda bilinçli olduğunu açıkladı.
- Kullanıcı ayrıca kuralı netleştirdi: KDV yoksa tevkifat yok; KDV varsa tevkifat da var. Mevcut `IsVatExempt` modeli bu kuralla uyumludur; bağımsız bir tevkifat istisnası eklenmedi.
- Kaynak formülleri ve dosyada saklanan Excel sonuçları okundu. Kaynak dosyalar değiştirilmedi. Bu çalışma Excel formüllerini yeniden yazma veya native Excel'de yeniden hesaplama iddiası içermez.
- Karşılaştırma, uygulamanın gerçek `ProgressPaymentCalculator` çekirdeğiyle hem iki ondalıklı gösterimde hem de ham değerlerde `0,0000001 TL` toleransıyla yapıldı. Bu tolerans Excel'in ikili kayan nokta artıklarını ayırır; kuruş farkını gizlemez.
- Paylaşılabilir JSON test verisinde kişilerin/firma sahiplerinin adları, gerçek hat adları ve vergi kimlikleri bulunmaz. Sayısal örnekler, kaynak dosya/sayfa/hücre adresleri ve hashler bulunur.

## Sonuçların ayrımı

- 422 alanda kaynak sonuçlarıyla eşleşme.
- Her iki dosyanın Taşıma!M21 hücresi boş: karşılaştırılabilir fatura referansı bulunmuyor; bu bir formül hatası olarak sınıflandırılmadı.
- Her iki dosyanın Taşıma!M23 hücresi `=I23`: kaynak fatura gösterimi matrahı (`38.101,03 TL`) içeriyor. Çekirdekteki `InvoiceAmountAfterWithholding` ise matrah + tevkifat (`40.006,0815 TL`) alanıdır. Ayrı anlam taşıyan bu iki değer eşitmiş gibi raporlanmadı. Kullanıcının tevkifat açıklaması nedeniyle kaynak formülü değiştirilmedi.
- Kaynak net ödemelerin tamamı kontrol edildi. Her dosya için taşıma toplamı `452.706,15335656 TL` (gösterim `452.706,15 TL`), özel eğitim toplamı `154.815,542094912 TL` (gösterim `154.815,54 TL`).

## Uygulama değişiklikleri

- Kullanıcının açık onayıyla hesap girişlerinde dört ondalık basamak korunuyor. Hat günlük fiyatları, form, geçmiş/arşiv düzeltmeleri ve hibrit hücre doğrulamaları birlikte ele alındı. Beşinci yeni ondalık basamak reddedilir. Sözleşme bedelinin iki basamaklı giriş kuralı korundu; para çıktıları iki ondalık gösterilir.
- Kayıtlı hakedişler kaydırılabilir salt okunur tabloya dönüştürüldü. Solda sabit işlem sütununda Düzenle, Sil ve Hesaplamayı Görüntüle bulunur. Klasör eylemi ayrıca korunur.
- Düzenle ve Sil mevcut kimlik + program türü çözümlemesini, düzeltme/arşiv akışını ve silme onayını kullanır.
- Hesap ayrıntıları penceresi girişleri, kayıtlı oranları, taşıma/rehber hesaplarını, KDV/tevkifatı, ek teminatı, damga vergisini, tüm borç/ceza kesintilerini, hesaplama netini ve resmî rapor zincirini gösterir.
- Hesap ayrıntılarının kaynağı kayıtlı `Calculation` ve `AppliedConstants` değerleridir. Güncel ayarlar eski hakedişe uygulanmaz. Eski kayıtta oran yoksa açıkça belirtilir. Resmî rapor kümülatifleri mevcut hakediş geçmişine göre açıklanır; bunlar geçmiş PDF dosyasının değişmez anlık görüntüsü olarak sunulmaz.
- Dört basamaklı bordro girdileri gerçek hibrit satır modeli ve form ayrıştırıcısıyla kontrol edildi. 42 örnek geçici program veri deposuna kaydedilip geri okunarak giriş, sonuç ve oranların korunması denetlendi. Gerçek kullanıcı veri deposu kullanılmadı.
- Hesap ayrıntısı gerçek WPF penceresinde açıldı; son hesap satırına kaydırma denetlendi. İşlem tablosu altı satırla ve minimum ekran genişliği dahil mevcut yerleşim testleriyle denetlendi. Render görüntüleri `.superpowers/payment-details-2026-10-26/` altındadır.

## Doğrulama kaydı

- Release çözümü: 270 çekirdek + 213 WPF testi, toplam 483 başarılı, 0 başarısız. Kanıt: iki test projesindeki `TestResults/payroll-details-final-2026-10-26.trx`.
- Kullanıcının tevkifat açıklamasıyla referans sınıflandırması güncellendikten sonra bordro kabul testi yeniden çalıştırıldı: 1/1 başarılı (`payroll-clarification-2026-10-26.trx`).
- Kaynak dosyaların SHA256 değerleri çalışmanın sonunda yeniden ölçüldü; başlangıçla aynı.
- Gerçek WPF tablo/pencere renderları görsel olarak incelendi. %150 DPI kontrolü yapılmış sayılmadı.

## Sınırlar

Bu sonuç, sağlanan örnekler ve otomatik regresyon kapsamı için geçerlidir. Bütün olası mali durumların hatasızlığı iddia edilmez. Önceki gerçek %150 / 144 DPI kabul maddesi bu çalışmayla kapatılmadı. `dist` ve GitHub sürüm varlıkları bu değişiklikler için yeniden paketlenmedi veya yayımlanmadı.
