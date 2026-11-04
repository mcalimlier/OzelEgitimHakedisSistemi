# Kayıtlar, arşiv ve ayarlar sadeleştirme planı

**Amaç:** Kullanıcının gösterdiği beş menü öğesini işlev kaybı olmadan azaltmak; kayıt işlemlerini ve filtre kapsamını anlaşılır kılmak.

**Tasarım:** Hakediş Geçmişi ve Arşivler, Kayıtlar ve Arşiv ekranında iki sekme olur. Canlı kayıtlarla değişiklik geçmişi saklanan arşivler birbirine dönüştürülmez. Ortak ve açık eğitim türü seçimi bulunur. Her sekmede sık kullanılan arama/dönem filtreleri görünür; ayrıntılı İKN/firma/durum filtreleri açılır alana alınır. Tablo esner, işlem düğmeleri altta görünür kalır. İstatistikler boş yer tutucusu kaldırılır. Geçici Hesap Tutucu, Hakediş Oluştur içinden açılan Canlı Hesap Dökümü olur; geri dönüş düğmesi bulunur. Ayarlar gerçek işlevleri içeren üç sade bölüme ayrılır: yedekleme, pencere görünümü, hesaplama sabitleri.

**Teknoloji:** Mevcut WPF/.NET 8, lacivert-altın tema ve kalıcı veri servisleri korunur. Finansal formüller ve dört PDF şablonu bu çalışmanın dışında kalır. Eski menü hedefleri History/Archives uyumluluk için aynı birleşik ekrana yönlenir.

- [x] Yanlış boş durum, örtük program filtresi ve sekme/menü erişimi için regresyon kontrollerini yaz ve sorunları doğrula.
- [x] Birleşik ekranı, bağımsız program seçimini, seçimi koruyan listeleri ve arşiv tablosunu uygula. Arşiv çağır/düzenle/klasör işlemlerini koru.
- [x] Kullanılmayan İstatistikler ekranını kaldır; canlı dökümü Hakediş Oluştur içine taşı ve menü etkinlik durumunu düzelt.
- [x] Ayarlar görünümünü sadeleştir; oran değişiklik kaydı ve yedek geri yükleme korumalarını incele. Bulunan hatalar için test ekle.
- [x] Dar/geniş pencere yerleşimlerini ve gerçek uygulamada sekme, filtre, seçim, canlı döküm ve geri dönüşü doğrula. Kullanıcı kayıtlarında silme/geri yükleme denemesi yapma.
- [x] Tüm testleri ve Visual Studio derlemesini sırayla çalıştır; inceleme raporunu ve doğru dala gönderilecek değişikliği hazırla.


Ek bulgu: deneme arşivlerinin tam yedek dışında kalması düzeltildi; eski dosyaları silmeyen aktarım, çakışma koruması ve geri yüklemeden sonra eski dosyaların yeniden içeri alınmaması test edildi. Gönderimden sonra GitHub push/PR sonuçları ayrıca izlenir.
