# Hatlar paneli ve GitHub kontrol incelemesi — 26 Ekim 2026

Çalışma dalı: `feature/official-payment-document-templates`. Kullanıcının istediği kod denetimi ve panel düzenlemesi aynı dalda yapıldı.

## GitHub Desktop işareti

`#3`, açık taslak pull request numarasıdır; hata sayısı değildir. Yanındaki rozet GitHub kontrol durumunu gösterir. PR kapatılmadı veya birleştirilmedi.

- İlk koşu **40388126660** tam çözümü **0 uyarı / 0 hata** ile derledi, fakat bütün ekrandaki `KAYIT DURUMU` yazılarını sayan eski denetim ikinci, ihale paneline ait etiketi de sayıp durdu.
- Denetim artık adlandırılmış `HeaderSaveStatusTextBlock` ve onun başlık kartını doğrular. Arşiv kontrolü de statik hale gelen `ProgressPaymentArchiveStore.SaveRevision` çağrısına güncellendi; revizyon koruma kontrolü kaldırılmadı.
- Dört salt okunur CI kontrolü yerelde çalıştırılıp doğrulandı. CI düzeltmesi **1b2fcb3** ile GitHub'a gönderildi.
- Sonraki koşu **40389407708**, statik kontrolleri ve çekirdek testlerini geçti; İngilizce Windows'ta `İzmir` için `izmir` aramasının boş dönmesini yakaladı. Firma/hat metin arama ve sıralamaları artık açık `tr-TR` kuralları kullanır. Firma araması hem `tr-TR` hem `en-US` testlerinde denetlenir; test beklentisi zayıflatılmadı.

## Kod düzeltmeleri

- İhale ile firmanın programı kaydederken karşılaştırılır. Kayıtlı hat başka eğitim programına taşınamaz; diğer program için yeni hat açılır.
- Düzenlenen hat silinmiş/kaybolmuşsa işlem başarıyla kaydedilmiş gibi gösterilmez; kayıt durdurulur.
- Sözleşme tarihindeki geçersiz metin, WPF eski tarihi geri yüklese bile kabul edilmez. Takvimden geçerli tarih seçildiğinde kayıt yeniden yapılabilir. Arşiv ve canlı hakediş düzeltme önizlemeleri de aynı kontrolü kullanır.
- Hat gün sayısı girişleri, biçimlendirmesi ve değişiklik karşılaştırması Türkçe sayı biçimine bağlandı. Önceki genel ayrıştırıcı İngilizce Windows'ta `20,5` değerini `205` okuyabiliyordu. Hat kaydetme, arşiv düzeltmesi ve önizlemelerinde 20 ondalık basamağa kadar Türkçe giriş kullanılır; iki program için `tr-TR`/`en-US` testleri bulunur. Diğer ekranların ayrıştırıcıları bu değişiklikte yeniden tanımlanmadı.
- Liste düğmeleri devam eden arşiv düzeltmesini bırakamaz veya bu sırada canlı hat silemez.
- İptal, kaydedilmemiş alanları temizler. Yeniden düzenlemede saklanan fiyatların hassasiyeti korunur.
- Rehber alanları normal ve arşiv düzenlemesinde hattın kendi programına göre gösterilir. Özel Eğitim rehber fiyatı, ihale kaydı bulunamadı diye sıfırlanmaz.
- Hat silme koruması hakediş bağlantısını program, ihale, sözleşme, hat ve firma kimliğiyle birlikte sayar. Diğer programdaki aynı numaralı hat, yanlışlıkla kullanılıyor sayılmaz.
- Onaylı silme işleminde ilgili kalıcı dönem taslakları ve bellek önbelleği temizlenir; hakediş/çıktı seçimleri yenilenir. Disk kaydı başarısızsa hat ve taslak kimlikleri birlikte geri alınır.

## Arayüz

- Boş üçüncü özet sütunu ve tekrarlanan akış kartı kaldırıldı. Üstte aynı yükseklikte ihale/firma seçimi, altında tam genişlikte hizalı alanlar ve en altta kayıt listesi bulunur.
- İhale seçenekleri iki programın kayıtlarını da erişilebilir tutar; seçenekte program adı gösterilir. Yüklenici seçenekleri seçili ihalenin programına göre süzülür.
- İhale seçilip firma seçilmediğinde o ihalenin tüm hatları görülebilir. Yeni kayıt için firma seçimi zorunludur.
- Hat no, hat adı ve yüklenici araması; görünen/toplam hat sayısı; boş liste açıklamaları eklendi. Arama taslak alanlarını ve saklanan koleksiyonları değiştirmez.
- Uzun isimler kartlarda satıra sarılır. Düzenle/sil düğmeleri metinden ayrıldı. Günlük fiyatlar dört ondalık basamağa kadar gösterilir; bilinmeyen eski sözleşme bilgileri "belirtilmedi" olarak işaretlenir.
- Hakediş/Excel başlıklarıyla ortak alan adları korundu. Uzun gün sayısı etiketi satıra sarılır; girişler aynı hizada kalır. Rehber alanı Temel Eğitimde yer kaplamaz.

## Doğrulama kayıtları

İlk beş regresyon senaryosu mevcut sürümde başarısız oldu (`.superpowers/route-red.log`). Program uyumsuzluğu, kayıp düzenleme kaydı, geçersiz tarih, arşiv koruması ve iptal davranışı testlerle yeniden üretildi.

Ekran genişliği kanıtları: `.superpowers/payment-details-2026-10-26/route-{1024,1366,1920}.png`. Gerçek Windows %150 / 144 DPI testi yerine sayılmaz. Finansal hesaplama formülleri ve `dist` dağıtım paketleri bu çalışmada değiştirilmedi.

- Son açık Visual Studio oturumunda **Tümünü Yeniden Derle / Debug Any CPU**: **4 başarılı proje, 0 başarısız; 0 hata, 0 uyarı**. Kanıt: `.superpowers/route-vs-final-build.log` ve `.superpowers/route-vs-final-state.txt`.
- Bu derleme üzerinde `dotnet test OzelEgitimHakedisSistemi.sln --no-build --no-restore -c Debug`: **270 çekirdek + 272 WPF = 542 başarılı, 0 başarısız, 0 atlanan**. Kanıt: `.superpowers/route-verified-tests.log`.
- **26 yeni hat paneli senaryosu**, ayrıca firma aramasına İngilizce Windows senaryosu eklendi. İptal sonrası yeniden açma testi, saklanan dört ondalıklı fiyatın korunmasını denetlemeye devam eder.
- İzole gerçek `App/MainWindow` kabulünde Özel/Temel Eğitim hatları gerçek Kaydet düğmesiyle kaydedildi; Düzenle/İptal düğmeleri, rehber alanı görünürlüğü, diskteki değerler ve hakediş hibrit satırındaki fiyatlar doğrulandı. Deneme programı verileri kendi `DenemeVerisi` dizinindedir. Ölçülen DPI: **96**. Kanıt: `.superpowers/RouteNativeSmoke/bin/Debug/net8.0-windows/route-smoke-result.txt` ve `route-native.png`.
- GitHub son kabul sonucu `.superpowers/route-ci-final.json` dosyasında ve çalışma sonu mesajındaki koşu bağlantısında kaydedilir. [Güncel dalın GitHub kontrolleri](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions?query=branch%3Afeature%2Fofficial-payment-document-templates). Kod commit'inden sonra PR'nin başarılı kontrol sonucu ayrıca izlenir; birleştirme/yayın yapılmaz.
